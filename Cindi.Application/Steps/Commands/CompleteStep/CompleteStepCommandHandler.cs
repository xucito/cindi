using Cindi.Application.Interfaces;
using Cindi.Application.Options;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Application.Steps.Commands.CreateStep;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Workflows;
using Cindi.Domain.Exceptions.WorkflowTemplates;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
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
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Cindi.Application.Steps.Commands.CompleteStep
{
    public class CompleteStepCommandHandler : IRequestHandler<CompleteStepCommand, CommandResult>
    {
        public IClusterStateService _clusterStateService;
        public ILogger<CompleteStepCommandHandler> Logger;
        private CindiClusterOptions _option;
        private IMediator _mediator;
        private readonly ApplicationDbContext _context;

        public CompleteStepCommandHandler(
            IClusterStateService clusterStateService,
            ILogger<CompleteStepCommandHandler> logger,
            IOptionsMonitor<CindiClusterOptions> options,
            IMediator mediator,
            ApplicationDbContext context
            )
        {
            _clusterStateService = clusterStateService;
            Logger = logger;
            _option = options.CurrentValue;
            options.OnChange((change) =>
            {
                _option = change;
            });
            _mediator = mediator;
            _context = context;
        }

        public CompleteStepCommandHandler(
            IClusterStateService clusterStateService,
            ILogger<CompleteStepCommandHandler> logger,
            CindiClusterOptions options,
            IMediator mediator,
             ApplicationDbContext context
    )
        {
            _clusterStateService = clusterStateService;
            Logger = logger;
            _option = options;
            _context = context;
            _mediator = mediator;
        }

        public async Task<CommandResult> Handle(CompleteStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var stepToComplete = await _context.Steps.FirstOrDefaultAsync<Step>(s => s.Id == request.Id);

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

            var stepTemplate = await _context.StepTemplates.FirstOrDefaultAsync<StepTemplate>(st => st.ReferenceId == stepToComplete.StepTemplateId);

            if (request.Outputs == null)
            {
                request.Outputs = new Dictionary<string, object>();
            }

            var botkey = await _context.BotKeys.FirstOrDefaultAsync<BotKey>(bk => bk.Id == request.BotId);

            var unencryptedOuputs = DynamicDataUtility.DecryptDynamicData(stepTemplate.OutputDefinitions, request.Outputs, EncryptionProtocol.RSA, botkey.PublicEncryptionKey, true);

            stepToComplete.Status = request.Status;
            stepToComplete.Outputs = DynamicDataUtility.EncryptDynamicData(stepTemplate.OutputDefinitions, unencryptedOuputs, EncryptionProtocol.AES256, ClusterStateService.GetEncryptionKey());
            stepToComplete.StatusCode = request.StatusCode;

            if (request.Log != null)
            {
                stepToComplete.Logs.Add(new StepLog()
                {
                    CreatedOn = DateTime.UtcNow,
                    Message = request.Log
                });
            }

            _context.Update(stepToComplete);
            await _context.SaveChangesAsync();

            Logger.LogInformation("Updated step " + stepToComplete.Id + " with status " + stepToComplete.Status);

            var updatedStep = await _context.Steps.FirstOrDefaultAsync<Step>(s => s.Id == stepToComplete.Id);

            if (updatedStep.WorkflowId != null)
            {
                var workflow = await _context.Workflows.FirstOrDefaultAsync<Workflow>(w => w.Id == updatedStep.WorkflowId.Value);

                if (workflow == null)
                {
                    throw new MissingWorkflowException("Failed to continue workflow " + updatedStep.WorkflowId + " as workflow does not exist.");
                }

                //Get the workflow template
                var workflowTemplate = await _context.WorkflowTemplates.FirstOrDefaultAsync<WorkflowTemplate>(wt => wt.ReferenceId == workflow.WorkflowTemplateId);

                if (workflowTemplate == null)
                {
                    throw new WorkflowTemplateNotFoundException("Failed to continue workflow " + updatedStep.WorkflowId + " workflow template " + workflow.WorkflowTemplateId + ".");
                }

                //Get all the steps related to this task
                var workflowSteps = (await _context.Steps.Where(s => s.WorkflowId == updatedStep.WorkflowId.Value).ToListAsync());

                foreach (var workflowStep in workflowSteps)
                {
                    workflowStep.Outputs = DynamicDataUtility.DecryptDynamicData((await _context.StepTemplates.FirstOrDefaultAsync<StepTemplate>(st => st.ReferenceId == workflowStep.StepTemplateId)).OutputDefinitions, workflowStep.Outputs, EncryptionProtocol.AES256, ClusterStateService.GetEncryptionKey());
                }
                //Keep track of whether a step has been added
                bool stepCreated = false;

                //Evaluate all logic blocks that have not been completed
                var logicBlocks = workflowTemplate.LogicBlocks.Where(lb => !workflow.CompletedLogicBlocks.Contains(lb.Key) && lb.Value.Dependencies.ContainsStep(updatedStep.Name)).ToList();

                foreach (var logicBlock in logicBlocks)
                {
                    Logger.LogInformation("Processing logic block " + logicBlock.Key + " for workflow " + workflow.Id);
                    var lockId = Guid.NewGuid();
                    bool lockObtained = false;
                    while (!lockObtained)
                    {
                        while (_clusterStateService.IsLogicBlockLocked(updatedStep.WorkflowId.Value, logicBlock.Key))
                        {
                            Console.WriteLine("Found " + ("workflow:" + updatedStep.WorkflowId + ">logicBlock:" + logicBlock) + " Lock");
                            await Task.Delay(1000);
                        }

                        int entryNumber = await _clusterStateService.LockLogicBlock(lockId, updatedStep.WorkflowId.Value, logicBlock.Key);

                        //Check whether you got the lock
                        lockObtained = _clusterStateService.WasLockObtained(lockId, updatedStep.WorkflowId.Value, logicBlock.Key);
                    }

                    //When the logic block is released, recheck whether this logic block has been evaluated
                    workflow = await _context.Workflows.FirstOrDefaultAsync<Workflow>(w => w.Id == updatedStep.WorkflowId.Value);
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

                        workflow.CompletedLogicBlocks.Add(logicBlock.Key);
                        _context.Update(workflow);
                        await _context.SaveChangesAsync();
                    }
                    await _clusterStateService.UnlockLogicBlock(lockId, updatedStep.WorkflowId.Value, logicBlock.Key);
                }

                //Check if there are no longer any steps that are unassigned or assigned

                var workflowStatus = workflow.Status;
                workflowSteps = (await _context.Steps.Where(s => s.WorkflowId == updatedStep.WorkflowId.Value).ToListAsync());
                var highestStatus = StepStatuses.GetHighestPriority(workflowSteps.Select(s => s.Status).ToArray());
                var newWorkflowStatus = stepCreated ? WorkflowStatuses.ConvertStepStatusToWorkflowStatus(StepStatuses.Unassigned) : WorkflowStatuses.ConvertStepStatusToWorkflowStatus(highestStatus);

                if (newWorkflowStatus != workflow.Status)
                {
                    workflow.Status = newWorkflowStatus;
                    _context.Update(workflow);
                    await _context.SaveChangesAsync();
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
