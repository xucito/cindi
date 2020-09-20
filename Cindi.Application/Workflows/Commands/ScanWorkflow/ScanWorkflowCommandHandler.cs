
using Cindi.Application.Interfaces;
using Cindi.Application.Options;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterOperation;
using Cindi.Application.Services.ClusterState;
using Cindi.Application.Steps.Commands.CompleteStep;
using Cindi.Application.Steps.Commands.CreateStep;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Exceptions.Workflows;
using Cindi.Domain.Exceptions.WorkflowTemplates;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Node.Communication.Controllers;
using ConsensusCore.Node.Services.Raft;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Workflows.Commands.ScanWorkflow
{
    public class ScanWorkflowCommandHandler : IRequestHandler<ScanWorkflowCommand, CommandResult>
    {
        public IClusterStateService _clusterStateService;
        public ILogger<ScanWorkflowCommandHandler> Logger;
        private CindiClusterOptions _option;
        private IMediator _mediator;
        private ClusterService _clusterService;

        public ScanWorkflowCommandHandler(
            IClusterStateService clusterStateService,
            ILogger<ScanWorkflowCommandHandler> logger,
            IOptionsMonitor<CindiClusterOptions> options,
            IMediator mediator,
            ClusterService clusterService
            )
        {
            _clusterService = clusterService;
            _clusterStateService = clusterStateService;
            Logger = logger;
            _option = options.CurrentValue;
            options.OnChange((change) =>
            {
                _option = change;
            });
            _mediator = mediator;
        }

        public async Task<CommandResult> Handle(ScanWorkflowCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            List<string> messages = new List<string>();
            bool workflowStillRunning = false;

            var workflow = await _clusterService.GetFirstOrDefaultAsync<Workflow>(w => w.Id == request.WorkflowId);

            if (workflow == null)
            {
                throw new MissingWorkflowException("Failed to find workflow " + request.WorkflowId + " as workflow does not exist.");
            }

            //Get the workflow template
            var workflowTemplate = await _clusterService.GetFirstOrDefaultAsync<WorkflowTemplate>(wt => wt.ReferenceId == workflow.WorkflowTemplateId);

            if (workflowTemplate == null)
            {
                throw new WorkflowTemplateNotFoundException("Failed to scan workflow " + request.WorkflowId + " workflow template " + workflow.WorkflowTemplateId + ".");
            }

            //Get all the steps related to this task
            var workflowSteps = (await _clusterService.GetAsync<Step>(s => s.WorkflowId == request.WorkflowId)).ToList();

            foreach (var workflowStep in workflowSteps)
            {
                workflowStep.Outputs = DynamicDataUtility.DecryptDynamicData((await _clusterService.GetFirstOrDefaultAsync<StepTemplate>(st => st.ReferenceId == workflowStep.StepTemplateId)).OutputDefinitions, workflowStep.Outputs, EncryptionProtocol.AES256, ClusterStateService.GetEncryptionKey());
                if (!workflowStep.IsComplete())
                {
                    messages.Add("Workflow step " + workflowStep.Id + " (" + workflowStep.Name + ")" + " is running.");
                    workflowStillRunning = true;
                }
            }

            bool stepCreated = false;

            if (!workflowStillRunning)
            {
                if(workflow.CompletedLogicBlocks == null)
                {
                    workflow.CompletedLogicBlocks = new List<string>();
                }

                //Evaluate all logic blocks that have not been completed
                var logicBlocks = workflowTemplate.LogicBlocks.Where(lb => !workflow.CompletedLogicBlocks.Contains(lb.Key)).ToList();

                foreach (var logicBlock in logicBlocks)
                {
                    var lockId = Guid.NewGuid();
                    bool lockObtained = false;
                    while (!lockObtained)
                    {
                        while (_clusterStateService.IsLogicBlockLocked(request.WorkflowId, logicBlock.Key))
                        {
                            Console.WriteLine("Found " + ("workflow:" + request.WorkflowId + ">logicBlock:" + logicBlock) + " Lock");
                            await Task.Delay(1000);
                        }

                        int entryNumber = await _clusterStateService.LockLogicBlock(lockId, request.WorkflowId, logicBlock.Key);
                        //Check whether you got the lock
                        lockObtained = _clusterStateService.WasLockObtained(lockId, request.WorkflowId, logicBlock.Key);
                    }

                    //When the logic block is released, recheck whether this logic block has been evaluated
                    workflow = await _clusterService.GetFirstOrDefaultAsync<Workflow>(w => w.Id == request.WorkflowId);
                    workflow.Inputs = DynamicDataUtility.DecryptDynamicData(workflowTemplate.InputDefinitions, workflow.Inputs, EncryptionProtocol.AES256, ClusterStateService.GetEncryptionKey());


                    if (workflow.CompletedLogicBlocks == null)
                    {
                        workflow.CompletedLogicBlocks = new List<string>();
                    }

                    //If the logic block is ready to be processed, submit the steps
                    if (logicBlock.Value.Dependencies.Evaluate(workflowSteps) &&  !workflow.CompletedLogicBlocks.Contains(logicBlock.Key))
                    {
                        foreach (var substep in logicBlock.Value.SubsequentSteps)
                        {
                            if (workflowSteps.Where(s => s.Name == substep.Key).Count() == 0)
                            {

                                var verifiedInputs = new Dictionary<string, object>();

                                foreach (var mapping in substep.Value.Mappings)
                                {
                                    //find the mapping with the highest priority
                                    var highestPriorityReference = WorkflowTemplateUtility.GetHighestPriorityReference(mapping.Value.OutputReferences, workflowSteps.ToArray());
                                    //if the highest priority reference is null, there are no mappings
                                    if (highestPriorityReference == null && mapping.Value.DefaultValue == null)
                                    {

                                    }
                                    // If default value is higher priority
                                    else if (mapping.Value.DefaultValue != null && (highestPriorityReference == null || highestPriorityReference.Priority < mapping.Value.DefaultValue.Priority))
                                    {
                                        verifiedInputs.Add(mapping.Key, mapping.Value.DefaultValue.Value);
                                    }
                                    // If the step ref is not -1 it is a step in the array but the workflow
                                    else if (highestPriorityReference.StepName != ReservedValues.WorkflowStartStepName)
                                    {
                                        var parentStep = workflowSteps.Where(ss => ss.Name == highestPriorityReference.StepName).FirstOrDefault();

                                        //If there is a AND and there is no parent step, throw a error
                                        if (parentStep == null)
                                        {
                                            throw new InvalidWorkflowProcessingException("Missing source for mapping " + mapping.Key + " from step " + highestPriorityReference.StepName);
                                        }
                                        else
                                        {
                                            if (parentStep != null)
                                            {
                                                try
                                                {
                                                    // Get the output value based on the pre-requisite id
                                                    var outPutValue = DynamicDataUtility.GetData(parentStep.Outputs, highestPriorityReference.OutputId);
                                                    // Validate it is in the definition
                                                    verifiedInputs.Add(mapping.Key, outPutValue.Value);

                                                }
                                                catch (Exception e)
                                                {
                                                    //TO DO Move this to logger
                                                    Console.WriteLine("Found error at mapping " + mapping.Key + " for step " + substep.Key);
                                                    throw e;
                                                }
                                            }
                                        }
                                    }
                                    //Get the value from the workflow ref
                                    else
                                    {
                                        // Validate it is in the definition
                                        verifiedInputs.Add(mapping.Key, DynamicDataUtility.GetData(workflow.Inputs, highestPriorityReference.OutputId).Value);
                                    }
                                }
                                stepCreated = true;
                                //Add the step TODO, add step priority
                                await _mediator.Send(new CreateStepCommand()
                                {
                                    StepTemplateId = substep.Value.StepTemplateId,
                                    CreatedBy = SystemUsers.QUEUE_MANAGER,
                                    Inputs = verifiedInputs,
                                    WorkflowId = workflow.Id,
                                    Name = substep.Key //create the step with the right subsequent step
                                });
                                messages.Add("Started workflow step " + substep.Key);
                            }
                        }

                        //Mark it as evaluated
                        workflow.CompletedLogicBlocks.Add(logicBlock.Key);
                        workflow.Version++;
                        //await _workflowsRepository.UpdateWorkflow(workflow);
                        await _clusterService.AddWriteOperation(new EntityWriteOperation<Workflow>()
                        {
                            Data = workflow,
                            WaitForSafeWrite = true,
                            Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Update,
                            User = request.CreatedBy
                        });
                    }
                    await _clusterStateService.UnlockLogicBlock(lockId, request.WorkflowId, logicBlock.Key);
                }

                //Check if there are no longer any steps that are unassigned or assigned

                var workflowStatus = workflow.Status;
                workflowSteps = (await _clusterService.GetAsync<Step>(s => s.WorkflowId == request.WorkflowId)).ToList();
                var highestStatus = StepStatuses.GetHighestPriority(workflowSteps.Select(s => s.Status).ToArray());
                var newWorkflowStatus = stepCreated ? WorkflowStatuses.ConvertStepStatusToWorkflowStatus(StepStatuses.Unassigned) : WorkflowStatuses.ConvertStepStatusToWorkflowStatus(highestStatus);

                if (newWorkflowStatus != workflow.Status)
                {
                    workflow.Status = newWorkflowStatus;

                    //await _workflowsRepository.UpdateWorkflow(workflow);
                    await _clusterService.AddWriteOperation(new EntityWriteOperation<Workflow>()
                    {
                        Data = workflow,
                        WaitForSafeWrite = true,
                        Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Update,
                        User = request.CreatedBy
                    });


                    messages.Add("Updated workflow status " + newWorkflowStatus + ".");
                }
            }

            return new CommandResult()
            {
                ObjectRefId = request.WorkflowId.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Update,
                IsSuccessful = true,
                Messages = messages.ToArray()
            };
        }
    }
}
