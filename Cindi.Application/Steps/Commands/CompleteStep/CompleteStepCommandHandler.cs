using Cindi.Application.Interfaces;
using Cindi.Application.Options;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Application.Steps.Commands.CreateStep;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Sequences;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Sequences;
using Cindi.Domain.Exceptions.SequenceTemplates;
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

namespace Cindi.Application.Steps.Commands.CompleteStep
{
    public class CompleteStepCommandHandler : IRequestHandler<CompleteStepCommand, CommandResult>
    {
        public IStepsRepository _stepsRepository;
        public IStepTemplatesRepository _stepTemplatesRepository;
        public ISequenceTemplatesRepository _sequenceTemplateRepository;
        public ISequencesRepository _sequencesRepository;
        public IClusterStateService _clusterStateService;
        public ILogger<CompleteStepCommandHandler> Logger;
        private CindiClusterOptions _option;
        private IMediator _mediator;
        private IBotKeysRepository _botKeysRepository;
        public CompleteStepCommandHandler(IStepsRepository stepsRepository,
            IStepTemplatesRepository stepTemplatesRepository,
            ISequenceTemplatesRepository sequenceTemplateRepository,
            ISequencesRepository sequencesRepository,
            IClusterStateService clusterStateService,
            ILogger<CompleteStepCommandHandler> logger,
            IOptionsMonitor<CindiClusterOptions> options,
            IMediator mediator,
            IBotKeysRepository botKeysRepository
            )
        {
            _stepsRepository = stepsRepository;
            _stepTemplatesRepository = stepTemplatesRepository;
            _sequenceTemplateRepository = sequenceTemplateRepository;
            _sequencesRepository = sequencesRepository;
            _clusterStateService = clusterStateService;
            Logger = logger;
            _option = options.CurrentValue;
            options.OnChange((change) =>
            {
                _option = change;
            });
            _mediator = mediator;
            _botKeysRepository = botKeysRepository;
        }

        public CompleteStepCommandHandler(IStepsRepository stepsRepository,
            IStepTemplatesRepository stepTemplatesRepository,
            ISequenceTemplatesRepository sequenceTemplateRepository,
            ISequencesRepository sequencesRepository,
            IClusterStateService clusterStateService,
            ILogger<CompleteStepCommandHandler> logger,
            CindiClusterOptions options,
            IMediator mediator,
            IBotKeysRepository botKeysRepository
    )
        {
            _stepsRepository = stepsRepository;
            _stepTemplatesRepository = stepTemplatesRepository;
            _sequenceTemplateRepository = sequenceTemplateRepository;
            _sequencesRepository = sequencesRepository;
            _clusterStateService = clusterStateService;
            Logger = logger;
            _option = options;
            _botKeysRepository = botKeysRepository;
        }

        public async Task<CommandResult> Handle(CompleteStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var stepToComplete = await _stepsRepository.GetStepAsync(request.Id);

            if (stepToComplete.IsComplete())
            {
                throw new InvalidStepStatusInputException("Step " + request.Id + " is already complete with status " + stepToComplete.Status + ".");
            }

            if (request.Status == StepStatuses.Suspended)
            {
                stepToComplete.UpdateJournal(new Domain.Entities.JournalEntries.JournalEntry()
                {
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = request.CreatedBy,
                    Updates = new List<Domain.ValueObjects.Update>()
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
                                FieldName = "suspendeduntil",
                                Value = request.SuspendFor == null ?DateTime.UtcNow.AddMilliseconds( _option.DefaultSuspensionTime) : DateTime.UtcNow.AddMilliseconds(DateTimeMathsUtility.GetMs(request.SuspendFor))
                            }
                        }
                });
                await _stepsRepository.UpdateStep(stepToComplete);


                return new CommandResult()
                {
                    ObjectRefId = request.Id.ToString(),
                    ElapsedMs = stopwatch.ElapsedMilliseconds,
                    Type = CommandResultTypes.Update
                };
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

            List<string> keysToChange = new List<string>();
            foreach (var output in request.Outputs)
            {
                if (output.Key == InputDataTypes.Secret)
                {
                    keysToChange.Add(output.Key);
                }
            }

            foreach (var key in keysToChange)
            {
                request.Outputs[key] = SecurityUtility.SymmetricallyEncrypt((string)request.Outputs[key], ClusterStateService.GetEncryptionKey());
            }
            var botkey = await _botKeysRepository.GetBotKeyAsync(request.BotId);

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
                                Value = InputDataUtility.EncryptDynamicData(stepTemplate,InputDataUtility.DecryptDynamicData(stepTemplate,request.Outputs, EncryptionProtocol.RSA, botkey.PublicEncryptionKey, true), EncryptionProtocol.AES256, ClusterStateService.GetEncryptionKey())
                            },
                            new Update()
                            {
                                Type = UpdateType.Override,
                                FieldName = "statuscode",
                                Value = request.StatusCode,
                            }
                        };
            if(request.Log != null)
            {
                finalUpdate.Add(
                            new Update()
                            {
                                Type = UpdateType.Append,
                                FieldName = "logs",
                                Value = new StepLog() {
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

            var updatedStepId = await _stepsRepository.UpdateStep(stepToComplete);
            var updatedStep = await _stepsRepository.GetStepAsync(stepToComplete.Id);

            if (updatedStep.SequenceId != null)
            {
                //Keep track of whether a step has been added
                var addedStep = false;
                var sequence = await _sequencesRepository.GetSequenceAsync(updatedStep.SequenceId.Value);

                if (sequence == null)
                {
                    throw new MissingSequenceException("Failed to continue sequence " + updatedStep.SequenceId + " as sequence does not exist.");
                }

                //Get the sequence template
                var sequenceTemplate = await _sequenceTemplateRepository.GetSequenceTemplateAsync(sequence.SequenceTemplateId);

                if (sequenceTemplate == null)
                {
                    throw new SequenceTemplateNotFoundException("Failed to continue sequence " + updatedStep.SequenceId + " sequence template " + sequence.SequenceTemplateId + ".");
                }

                //Get all the steps related to this task
                var sequenceSteps = await _sequencesRepository.GetSequenceStepsAsync(updatedStep.SequenceId.Value);

                //Get all the logic blocks for all logic blocks containing the step that has been completed
                var logicBlocks = sequenceTemplate.LogicBlocks.Where(lb => lb.PrerequisiteSteps.Where(ps => ps.StepRefId == updatedStep.StepRefId).Count() > 0).ToList();
                

                foreach (var logicBlock in logicBlocks)
                {
                    while (_clusterStateService.IsLogicBlockLocked(updatedStep.SequenceId + ">logicBlock:" + logicBlock.Id))
                    {
                        Console.WriteLine("Found " + ("sequence:" + updatedStep.SequenceId + ">logicBlock:" + logicBlock) + " Lock");
                        Thread.Sleep(1000);
                    }

                    _clusterStateService.LockLogicBlock("sequence:" + updatedStep.SequenceId + ">logicBlock:" + logicBlock.Id);

                    var ready = true;

                    var Condition = logicBlock.Condition;

                    if (Condition == "AND")
                    {
                        foreach (var prerequisite in logicBlock.PrerequisiteSteps)
                        {
                            // Check whether the pre-requisite is in the submited steps, if there is a single one missing, do not submit subsequent steps 
                            var step = sequenceSteps.Where(s => s.StepRefId == prerequisite.StepRefId).FirstOrDefault();
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
                            var matchedSteps = sequenceSteps.Where(s => s.StepRefId == prerequisite.StepRefId);

                            if (matchedSteps.Count() > 1)
                            {
                                Logger.LogError("Detected duplicate pre-requisite steps for sequence " + sequence.Id + " at step reference " + prerequisite.StepRefId);
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
                    }

                    //If the logic block is ready to be processed, submit the steps
                    if (ready)
                    {
                        foreach (var substep in logicBlock.SubsequentSteps)
                        {
                            if (sequenceSteps.Where(s => s.StepRefId == substep.StepRefId).Count() > 0)
                            {
                                Logger.LogCritical("Encountered duplicate subsequent step for sequence " + sequence.Id + ". Ignoring the generation of duplicate.");
                            }
                            else
                            {

                                var verifiedInputs = new Dictionary<string, object>();

                                foreach (var mapping in substep.Mappings)
                                {
                                    //find the mapping with the highest priority
                                    var highestPriorityReference = SequenceTemplateUtility.GetHighestPriorityReference(mapping.OutputReferences, sequenceSteps.ToArray());


                                    //if the highest priority reference is null, there are no mappings
                                    if (highestPriorityReference == null && mapping.DefaultValue == null)
                                    {

                                    }
                                    // If default value is higher priority
                                    else if (mapping.DefaultValue != null && (highestPriorityReference == null || highestPriorityReference.Priority < mapping.DefaultValue.Priority))
                                    {
                                        verifiedInputs.Add(mapping.StepInputId, mapping.DefaultValue.Value);
                                    }
                                    // If the step ref is not -1 it is a step in the array but the sequence
                                    else if (highestPriorityReference.StepRefId != -1)
                                    {
                                        var parentStep = sequenceSteps.Where(ss => ss.StepRefId == highestPriorityReference.StepRefId).FirstOrDefault();

                                        //If there is a AND and there is no parent step, throw a error
                                        if (parentStep == null && Condition == "AND")
                                        {
                                            throw new InvalidSequenceProcessingException("Detected AND Condition but no parent found");
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
                                                    verifiedInputs.Add(mapping.StepInputId, outPutValue.Value);

                                                }
                                                catch (Exception e)
                                                {
                                                    //TO DO Move this to logger
                                                    Console.WriteLine("Found error at mapping " + mapping.StepInputId + " for step " + substep.StepRefId);
                                                    throw e;
                                                }
                                            }
                                        }
                                    }
                                    //Get the value from the sequence ref
                                    else
                                    {
                                        // Validate it is in the definition
                                        verifiedInputs.Add(mapping.StepInputId, DynamicDataUtility.GetData(sequence.Inputs, highestPriorityReference.OutputId).Value);
                                    }

                                }


                                //Add the step TODO, add step priority
                                await _mediator.Send(new CreateStepCommand()
                                {
                                    StepTemplateId = substep.StepTemplateId,
                                    CreatedBy = SystemUsers.QUEUE_MANAGER,
                                    Inputs = verifiedInputs,
                                    SequenceId = sequence.Id,
                                    StepRefId = substep.StepRefId
                                });
                                addedStep = true;
                            }
                        }
                        _clusterStateService.UnlockLogicBlock("sequence:" + updatedStep.SequenceId + ">logicBlock:" + logicBlock.Id);
                    }

                }

                //Check if there are no longer any steps that are unassigned or assigned

                var sequenceStatus = sequence.Status;
                sequenceSteps = await _sequencesRepository.GetSequenceStepsAsync(updatedStep.SequenceId.Value);
                var highestStatus = StepStatuses.GetHighestPriority(sequenceSteps.Select(s => s.Status).ToArray());

                var newSequenceStatus = SequenceStatuses.ConvertStepStatusToSequenceStatus(highestStatus);

                if (newSequenceStatus != sequence.Status)
                {

                    sequence.UpdateJournal(
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
                                    Value = newSequenceStatus
                                }
                        }
                    });

                    await _sequencesRepository.UpdateSequence(sequence);
                }
                await _sequencesRepository.UpsertSequenceMetadataAsync(sequence.Id);
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
