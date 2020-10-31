using Cindi.Application.Interfaces;
using Cindi.Application.Options;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Application.Steps.Commands.CreateStep;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Workflows;
using Cindi.Domain.Exceptions.WorkflowTemplates;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.RPCs;
using ConsensusCore.Node;
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
using Newtonsoft.Json;
using ConsensusCore.Node.Communication.Controllers;
using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Node.Services.Raft;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Application.Services.ClusterOperation;

namespace Cindi.Application.Steps.Commands.CompleteStep
{
    public class CompleteStepCommandHandler : IRequestHandler<CompleteStepCommand, CommandResult>
    {
        public IClusterStateService _clusterStateService;
        public ILogger<CompleteStepCommandHandler> Logger;
        private CindiClusterOptions _option;
        private IClusterService _clusterService;
        private IMediator _mediator;

        public CompleteStepCommandHandler(
            IClusterStateService clusterStateService,
            ILogger<CompleteStepCommandHandler> logger,
            IOptionsMonitor<CindiClusterOptions> options,
            IClusterService clusterService,
            IMediator mediator
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
            _clusterService = clusterService;
            _mediator = mediator;
        }

        public CompleteStepCommandHandler(
            IClusterStateService clusterStateService,
            ILogger<CompleteStepCommandHandler> logger,
            CindiClusterOptions options,
            IClusterService clusterService,
            IMediator mediator
    )
        {
            _clusterService = clusterService;
            _clusterStateService = clusterStateService;
            Logger = logger;
            _option = options;
            _mediator = mediator;
        }

        public async Task<CommandResult> Handle(CompleteStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var stepToComplete = await _clusterService.GetFirstOrDefaultAsync<Step>(s => s.Id == request.Id);

            if (stepToComplete.IsComplete())
            {
                // if (JsonConvert.SerializeObject(stepToComplete.Outputs) == JsonConvert.SerializeObject(request.Outputs))
                // {
                Logger.LogWarning("Step " + request.Id + " is already complete with status " + stepToComplete.Status + ".");
                throw new DuplicateStepUpdateException();
                //}

            }

            if (!StepStatuses.IsCompleteStatus(request.Status))
            {
                throw new InvalidStepStatusInputException(request.Status + " is not a valid completion status.");
            }

            var stepTemplate = await _clusterService.GetFirstOrDefaultAsync<StepTemplate>(st => st.ReferenceId == stepToComplete.StepTemplateId);

            if (request.Outputs == null)
            {
                request.Outputs = new Dictionary<string, object>();
            }

            var botkey = await _clusterService.GetFirstOrDefaultAsync<BotKey>(bk => bk.Id == request.BotId);

            var unencryptedOuputs = DynamicDataUtility.DecryptDynamicData(stepTemplate.OutputDefinitions, request.Outputs, EncryptionProtocol.AES256, SecurityUtility.RsaDecryptWithPublic(request.EncryptionKey, botkey.PublicEncryptionKey), true);
            stepToComplete.Status = request.Status;
            stepToComplete.Outputs = DynamicDataUtility.EncryptDynamicData(stepTemplate.OutputDefinitions, unencryptedOuputs, EncryptionProtocol.AES256, ClusterStateService.GetEncryptionKey());
            stepToComplete.StatusCode = request.StatusCode;
            stepToComplete.Version++;
            if (request.Log != null)
            {
                stepToComplete.Logs.Add(new StepLog()
                {
                    CreatedOn = DateTime.UtcNow,
                    Message = request.Log
                });
            }


            await _clusterService.AddWriteOperation(new EntityWriteOperation<Step>()
            {
                Data = stepToComplete,
                WaitForSafeWrite = true,
                Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Update,
                User = request.CreatedBy
            });

            Logger.LogInformation("Updated step " + stepToComplete.Id + " with status " + stepToComplete.Status);

            var updatedStep = await _clusterService.GetFirstOrDefaultAsync<Step>(s => s.Id == stepToComplete.Id);

            if (updatedStep.WorkflowId != null)
            {
                var workflow = await _clusterService.GetFirstOrDefaultAsync<Workflow>(w => w.Id == updatedStep.WorkflowId.Value);

                if (workflow == null)
                {
                    throw new MissingWorkflowException("Failed to continue workflow " + updatedStep.WorkflowId + " as workflow does not exist.");
                }

                //Get the workflow template
                var workflowTemplate = await _clusterService.GetFirstOrDefaultAsync<WorkflowTemplate>(wt => wt.ReferenceId == workflow.WorkflowTemplateId);

                if (workflowTemplate == null)
                {
                    throw new WorkflowTemplateNotFoundException("Failed to continue workflow " + updatedStep.WorkflowId + " workflow template " + workflow.WorkflowTemplateId + ".");
                }

                //Get all the steps related to this task
                var workflowSteps = (await _clusterService.GetAsync<Step>(s => s.WorkflowId == updatedStep.WorkflowId.Value)).ToList();
                
                //Get all the steps related to this task
                foreach (var workflowStep in await _clusterService.GetAsync<Step>(s => s.WorkflowId == updatedStep.WorkflowId.Value))
                {
                    workflowStep.Outputs = DynamicDataUtility.DecryptDynamicData((await _clusterService.GetFirstOrDefaultAsync<StepTemplate>(st => st.ReferenceId == workflowStep.StepTemplateId)).OutputDefinitions, workflowStep.Outputs, EncryptionProtocol.AES256, ClusterStateService.GetEncryptionKey());
                }
                //Keep track of whether a step has been added
                bool stepCreated = false;

                //Evaluate all logic blocks that have not been completed
                foreach (var logicBlock in workflowTemplate.LogicBlocks.Where(lb => !workflow.CompletedLogicBlocks.Contains(lb.Key) && lb.Value.Dependencies.ContainsStep(updatedStep.Name)))
                {
                    Logger.LogInformation("Processing logic block " + logicBlock.Key + " for workflow " + workflow.Id);
                    var lockId = Guid.NewGuid();
                    bool lockObtained = false;
                    while (!lockObtained)
                    {
                        while (_clusterStateService.IsLogicBlockLocked(updatedStep.WorkflowId.Value, logicBlock.Key))
                        {
                            //Console.WriteLine("Found " + ("workflow:" + updatedStep.WorkflowId + ">logicBlock:" + logicBlock) + " Lock");
                            await Task.Delay(1000);
                        }

                        int entryNumber = await _clusterStateService.LockLogicBlock(lockId, updatedStep.WorkflowId.Value, logicBlock.Key);

                        //Check whether you got the lock
                        lockObtained = _clusterStateService.WasLockObtained(lockId, updatedStep.WorkflowId.Value, logicBlock.Key);
                    }

                    //When the logic block is released, recheck whether this logic block has been evaluated
                    workflow = await _clusterService.GetFirstOrDefaultAsync<Workflow>(w => w.Id == updatedStep.WorkflowId.Value);
                    workflow.Inputs = DynamicDataUtility.DecryptDynamicData(workflowTemplate.InputDefinitions, workflow.Inputs, EncryptionProtocol.AES256, ClusterStateService.GetEncryptionKey());

                    //If the logic block is ready to be processed, submit the steps
                    if (logicBlock.Value.Dependencies.Evaluate(workflowSteps) && !workflow.CompletedLogicBlocks.Contains(logicBlock.Key))
                    {
                        foreach (var substep in logicBlock.Value.SubsequentSteps)
                        {
                            if (workflowSteps.Where(s => s.Name == substep.Key).Count() > 0)
                            {
                                Logger.LogCritical("Encountered duplicate subsequent step for workflow " + workflow.Id + ". Ignoring the generation of duplicate.");
                            }
                            else
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
                            }
                        }

                        //Mark it as evaluated
                        workflow.CompletedLogicBlocks.Add(logicBlock.Key);

                        //await _workflowsRepository.UpdateWorkflow(workflow);
                        await _clusterService.AddWriteOperation(new EntityWriteOperation<Workflow>()
                        {
                            Data = workflow,
                            WaitForSafeWrite = true,
                            Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Update,
                            User = request.CreatedBy
                        });
                    }
                    await _clusterStateService.UnlockLogicBlock(lockId, updatedStep.WorkflowId.Value, logicBlock.Key);
                }

                //Check if there are no longer any steps that are unassigned or assigned

                var workflowStatus = workflow.Status;
                workflowSteps = (await _clusterService.GetAsync<Step>(s => s.WorkflowId == updatedStep.WorkflowId.Value)).ToList();
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
                }
            }

            return new CommandResult()
            {
                ObjectRefId = request.Id.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Update
            };
        }
    }
}
