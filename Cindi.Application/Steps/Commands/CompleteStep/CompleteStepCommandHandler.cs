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

namespace Cindi.Application.Steps.Commands.CompleteStep
{
    public class CompleteStepCommandHandler : IRequestHandler<CompleteStepCommand, CommandResult>
    {
        public IStepsRepository _stepsRepository;
        public IStepTemplatesRepository _stepTemplatesRepository;
        public IWorkflowTemplatesRepository _workflowTemplateRepository;
        public IWorkflowsRepository _workflowsRepository;
        public IClusterStateService _clusterStateService;
        public ILogger<CompleteStepCommandHandler> Logger;
        private CindiClusterOptions _option;
        private IMediator _mediator;
        private IBotKeysRepository _botKeysRepository;
        private readonly IConsensusCoreNode<CindiClusterState> _node;

        public CompleteStepCommandHandler(IStepsRepository stepsRepository,
            IStepTemplatesRepository stepTemplatesRepository,
            IWorkflowTemplatesRepository workflowTemplateRepository,
            IWorkflowsRepository workflowsRepository,
            IClusterStateService clusterStateService,
            ILogger<CompleteStepCommandHandler> logger,
            IOptionsMonitor<CindiClusterOptions> options,
            IMediator mediator,
            IBotKeysRepository botKeysRepository,
            IConsensusCoreNode<CindiClusterState> node
            )
        {
            _stepsRepository = stepsRepository;
            _stepTemplatesRepository = stepTemplatesRepository;
            _workflowTemplateRepository = workflowTemplateRepository;
            _workflowsRepository = workflowsRepository;
            _clusterStateService = clusterStateService;
            Logger = logger;
            _option = options.CurrentValue;
            options.OnChange((change) =>
            {
                _option = change;
            });
            _mediator = mediator;
            _botKeysRepository = botKeysRepository;
            _node = node;
        }

        public CompleteStepCommandHandler(IStepsRepository stepsRepository,
            IStepTemplatesRepository stepTemplatesRepository,
            IWorkflowTemplatesRepository workflowTemplateRepository,
            IWorkflowsRepository workflowsRepository,
            IClusterStateService clusterStateService,
            ILogger<CompleteStepCommandHandler> logger,
            CindiClusterOptions options,
            IMediator mediator,
            IBotKeysRepository botKeysRepository,
             IConsensusCoreNode<CindiClusterState> node
    )
        {
            _stepsRepository = stepsRepository;
            _stepTemplatesRepository = stepTemplatesRepository;
            _workflowTemplateRepository = workflowTemplateRepository;
            _workflowsRepository = workflowsRepository;
            _clusterStateService = clusterStateService;
            Logger = logger;
            _option = options;
            _botKeysRepository = botKeysRepository;
            _node = node;
            _mediator = mediator;
        }

        public async Task<CommandResult> Handle(CompleteStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var stepToComplete = await _stepsRepository.GetStepAsync(request.Id);

            if (stepToComplete.IsComplete())
            {
                if (JsonConvert.SerializeObject(stepToComplete.Outputs) == JsonConvert.SerializeObject(request.Outputs))
                {
                    throw new DuplicateStepUpdateException();
                }

                throw new InvalidStepStatusInputException("Step " + request.Id + " is already complete with status " + stepToComplete.Status + ".");
            }

            if (!StepStatuses.IsCompleteStatus(request.Status))
            {
                throw new InvalidStepStatusInputException(request.Status + " is not a valid completion status.");
            }

            var stepTemplate = await _stepTemplatesRepository.GetStepTemplateAsync(stepToComplete.StepTemplateId);

            if (request.Outputs == null)
            {
                request.Outputs = new Dictionary<string, object>();
            }

            var botkey = await _botKeysRepository.GetBotKeyAsync(request.BotId);

            var unencryptedOuputs = DynamicDataUtility.DecryptDynamicData(stepTemplate.OutputDefinitions, request.Outputs, EncryptionProtocol.RSA, botkey.PublicEncryptionKey, true);
            var finalUpdate = new List<Domain.ValueObjects.Update>()
                        {
                            new Update()
                            {
                                Type = UpdateType.Override,
                                FieldName = "status",
                                Value = request.Status,
                            },
                            new Update()
                            {
                                Type = UpdateType.Override,
                                FieldName = "outputs",
                                Value = DynamicDataUtility.EncryptDynamicData(stepTemplate.OutputDefinitions, unencryptedOuputs, EncryptionProtocol.AES256, ClusterStateService.GetEncryptionKey())
                            },
                            new Update()
                            {
                                Type = UpdateType.Override,
                                FieldName = "statuscode",
                                Value = request.StatusCode,
                            }
                        };
            if (request.Log != null)
            {
                finalUpdate.Add(
                            new Update()
                            {
                                Type = UpdateType.Append,
                                FieldName = "logs",
                                Value = new StepLog()
                                {
                                    CreatedOn = DateTime.UtcNow,
                                    Message = request.Log
                                },
                            });
            }

            stepToComplete.UpdateJournal(new Domain.Entities.JournalEntries.JournalEntry()
            {
                CreatedOn = DateTime.UtcNow,
                CreatedBy = request.CreatedBy,
                Updates = finalUpdate
            });

            //var updatedStepId = await _stepsRepository.UpdateStep(stepToComplete);

            await _node.Handle(new WriteData()
            {
                Data = stepToComplete,
                WaitForSafeWrite = true,
                Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Update
            });

            var updatedStep = await _stepsRepository.GetStepAsync(stepToComplete.Id);

            if (updatedStep.WorkflowId != null)
            {
                //Keep track of whether a step has been added
                var addedStep = false;
                var workflow = await _workflowsRepository.GetWorkflowAsync(updatedStep.WorkflowId.Value);

                if (workflow == null)
                {
                    throw new MissingWorkflowException("Failed to continue workflow " + updatedStep.WorkflowId + " as workflow does not exist.");
                }

                //Get the workflow template
                var workflowTemplate = await _workflowTemplateRepository.GetWorkflowTemplateAsync(workflow.WorkflowTemplateId);

                if (workflowTemplate == null)
                {
                    throw new WorkflowTemplateNotFoundException("Failed to continue workflow " + updatedStep.WorkflowId + " workflow template " + workflow.WorkflowTemplateId + ".");
                }

                //Get all the steps related to this task
                var workflowSteps = await _workflowsRepository.GetWorkflowStepsAsync(updatedStep.WorkflowId.Value);

                //Evaluate all logic blocks that have not been completed
                var logicBlocks = workflowTemplate.LogicBlocks.Where(lb => !workflow.CompletedLogicBlocks.Contains(lb.Key)).ToList();

                foreach (var logicBlock in logicBlocks)
                {
                    var lockId = Guid.NewGuid();
                    bool lockObtained = false;
                    while (!lockObtained)
                    {
                        while (_clusterStateService.IsLogicBlockLocked(updatedStep.WorkflowId.Value, logicBlock.Key))
                        {
                            Console.WriteLine("Found " + ("workflow:" + updatedStep.WorkflowId + ">logicBlock:" + logicBlock) + " Lock");
                            Thread.Sleep(1000);
                        }

                        int entryNumber = await _clusterStateService.LockLogicBlock(lockId, updatedStep.WorkflowId.Value, logicBlock.Key);

                        // CheckIfYouObtainedALock

                        while (!_node.HasEntryBeenCommitted(entryNumber))
                        {
                            Logger.LogDebug("Waiting for entry " + entryNumber + " to be commited.");
                            Thread.Sleep(250);
                        }

                        //Check whether you got the lock
                        lockObtained = _clusterStateService.WasLockObtained(lockId, updatedStep.WorkflowId.Value, logicBlock.Key);
                    }

                    //When the logic block is released, recheck whether this logic block has been evaluated
                    workflow = await _workflowsRepository.GetWorkflowAsync(updatedStep.WorkflowId.Value);

                    //var ready = true;
                    /*var Condition = logicBlock.Condition;

                    if (Condition == "AND")
                    {
                        foreach (var prerequisite in logicBlock.PrerequisiteSteps)
                        {
                            // Check whether the pre-requisite is in the submited steps, if there is a single one missing, do not submit subsequent steps 
                            var step = workflowSteps.Where(s => s.WorkflowStepId == prerequisite.WorkflowStepId).FirstOrDefault();
                            if (step == null)
                            {
                                ready = false;
                                break;
                            }
                            if (prerequisite.Status != step.Status || step.StatusCode != prerequisite.StatusCode)
                            {
                                ready = false;
                                break;
                            }
                        }
                    }
                    //One of the conditions must be true, only effective the first time
                    else if (Condition == "OR")
                    {
                        // In OR start off with No and set to Yes
                        ready = false;

                        foreach (var prerequisite in logicBlock.PrerequisiteSteps)
                        {

                            // Check whether the pre-requisite is in the submited steps, if there is a single one missing break but do not change ready
                            var matchedSteps = workflowSteps.Where(s => s.WorkflowStepId == prerequisite.WorkflowStepId);

                            if (matchedSteps.Count() > 1)
                            {
                                Logger.LogError("Detected duplicate pre-requisite steps for workflow " + workflow.Id + " at step reference " + prerequisite.WorkflowStepId);
                            }

                            if (matchedSteps == null)
                            {
                                break;
                            }
                            // If there are duplicates, match at least one to the next condition.
                            if (matchedSteps.Where(matchedStep => prerequisite.Status == matchedStep.Status && matchedStep.StatusCode == prerequisite.StatusCode).Count() > 0)
                            {
                                ready = true;
                                break;
                            }

                            if (ready)
                            {
                                break;
                            }
                        }
                    }*/

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


                                //Add the step TODO, add step priority
                                await _mediator.Send(new CreateStepCommand()
                                {
                                    StepTemplateId = substep.Value.StepTemplateId,
                                    CreatedBy = SystemUsers.QUEUE_MANAGER,
                                    Inputs = verifiedInputs,
                                    WorkflowId = workflow.Id,
                                    Name = substep.Key //create the step with the right subsequent step
                                });
                                addedStep = true;
                            }
                        }

                        //Mark it as evaluated
                        workflow.UpdateJournal(
                        new JournalEntry()
                        {
                            CreatedBy = request.CreatedBy,
                            CreatedOn = DateTime.UtcNow,
                            Updates = new List<Update>()
                            {
                                new Update()
                                {
                                    FieldName = "completedlogicblocks",
                                    Type = UpdateType.Append,
                                    Value = logicBlock.Key
                                }
                            }
                        });

                        //await _workflowsRepository.UpdateWorkflow(workflow);
                        await _node.Handle(new WriteData()
                        {
                            Data = workflow,
                            WaitForSafeWrite = true,
                            Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Update
                        });
                    }
                    await _clusterStateService.UnlockLogicBlock(lockId, updatedStep.WorkflowId.Value, logicBlock.Key);
                }

                //Check if there are no longer any steps that are unassigned or assigned

                var workflowStatus = workflow.Status;
                workflowSteps = await _workflowsRepository.GetWorkflowStepsAsync(updatedStep.WorkflowId.Value);
                var highestStatus = StepStatuses.GetHighestPriority(workflowSteps.Select(s => s.Status).ToArray());

                var newWorkflowStatus = WorkflowStatuses.ConvertStepStatusToWorkflowStatus(highestStatus);

                if (newWorkflowStatus != workflow.Status)
                {

                    workflow.UpdateJournal(
                    new JournalEntry()
                    {
                        CreatedBy = request.CreatedBy,
                        CreatedOn = DateTime.UtcNow,
                        Updates = new List<Update>()
                        {
                                new Update()
                                {
                                    FieldName = "status",
                                    Type = UpdateType.Override,
                                    Value = newWorkflowStatus
                                }
                        }
                    });

                    //await _workflowsRepository.UpdateWorkflow(workflow);
                    await _node.Handle(new WriteData()
                    {
                        Data = workflow,
                        WaitForSafeWrite = true,
                        Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Update
                    });
                }
                //await _workflowsRepository.UpsertWorkflowMetadataAsync(workflow.Id);
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
