using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Steps.Commands.AssignStep
{
    public class AssignStepCommandHandler : IRequestHandler<AssignStepCommand, CommandResult<Step>>
    {
        private readonly IStepsRepository _stepsRepository;
        private readonly IClusterStateService _clusterStateService;
        private IStepTemplatesRepository _stepTemplateRepository;
        private IBotKeysRepository _botKeysRepository;
        private IGlobalValuesRepository _globalValuesRepository;
        public ILogger<AssignStepCommandHandler> Logger;

        public AssignStepCommandHandler(
            IStepsRepository stepsRepository,
            IClusterStateService stateService,
            IStepTemplatesRepository stepTemplateRepository,
            IBotKeysRepository botKeysRepository,
            ILogger<AssignStepCommandHandler> logger,
            IGlobalValuesRepository globalValuesRepository
            )
        {
            _stepsRepository = stepsRepository;
            _clusterStateService = stateService;
            _stepTemplateRepository = stepTemplateRepository;
            _botKeysRepository = botKeysRepository;
            _globalValuesRepository = globalValuesRepository;
            Logger = logger;
        }
        public async Task<CommandResult<Step>> Handle(AssignStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (_clusterStateService.IsAssignmentEnabled())
            {
                var assignedStepSuccessfully = false;
                Step unassignedStep = null;
                var dateChecked = DateTime.UtcNow;
                do
                {
                    unassignedStep = (await _stepsRepository.GetStepsAsync(1, 0, StepStatuses.Unassigned, request.StepTemplateIds)).FirstOrDefault();
                    if (unassignedStep != null)
                    {
                        //Real values to pass to the Microservice
                        Dictionary<string, object> realAssignedValues = new Dictionary<string, object>();
                        //Inputs that have been converted to reference expression
                        Dictionary<string, object> convertedInputs = new Dictionary<string, object>();

                        var template = await _stepTemplateRepository.GetStepTemplateAsync(unassignedStep.StepTemplateId);
                        try
                        {
                            //This should not throw a error externally, the server should loop to the next one and log a error
                            if (unassignedStep.Status != StepStatuses.Unassigned)
                            {
                                throw new InvalidStepQueueException("You cannot assign step " + unassignedStep.Id + " as it is not unassigned.");
                            }


                            bool inputsUpdated = false;

                            foreach (var input in unassignedStep.Inputs)
                            {
                                string convertedValue = "";
                                bool isReferenceByValue = false;
                                var isReference = InputDataUtility.IsInputReference(input, out convertedValue, out isReferenceByValue);
                                if (input.Value is string && ((string)input.Value).Length > 1)
                                {
                                    if (isReference)
                                    {
                                        //Copy by reference
                                        if (isReferenceByValue)
                                        {
                                            var foundGlobalValue = await _globalValuesRepository.GetGlobalValueAsync(convertedValue);
                                            if (foundGlobalValue == null)
                                            {
                                                Logger.LogWarning("No global value was found for value " + input.Value);
                                                realAssignedValues.Add(input.Key, null);
                                                convertedInputs.Add(input.Key, input.Value + ":?");
                                            }
                                            else if (foundGlobalValue.Type != template.InputDefinitions[input.Key].Type)
                                            {
                                                Logger.LogWarning("Global value was found for value " + input.Value + " however they are different types. " + template.InputDefinitions[input.Key].Type + " vs " + foundGlobalValue.Type);
                                                realAssignedValues.Add(input.Key, null);
                                                convertedInputs.Add(input.Key, input.Value + ":?");
                                            }
                                            else
                                            {
                                                realAssignedValues.Add(input.Key, foundGlobalValue.Value);
                                                convertedInputs.Add(input.Key, input.Value + ":" + foundGlobalValue.Journal.GetCurrentChainId());
                                            }
                                        }
                                        //copy by value
                                        else
                                        {
                                            var foundGlobalValue = await _globalValuesRepository.GetGlobalValueAsync(convertedValue);
                                            if (foundGlobalValue == null)
                                            {
                                                Logger.LogWarning("No global value was found for value " + input.Value);
                                                realAssignedValues.Add(input.Key, null);
                                                convertedInputs.Add(input.Key, null);
                                            }
                                            else if (foundGlobalValue.Type != template.InputDefinitions[input.Key].Type)
                                            {
                                                Logger.LogWarning("Global value was found for value " + input.Value + " however they are different types. " + template.InputDefinitions[input.Key].Type + " vs " + foundGlobalValue.Type);
                                                realAssignedValues.Add(input.Key, null);
                                                convertedInputs.Add(input.Key, null);
                                            }
                                            else
                                            {
                                                realAssignedValues.Add(input.Key, foundGlobalValue.Value);
                                                convertedInputs.Add(input.Key, foundGlobalValue.Value);
                                            }
                                        }

                                        inputsUpdated = true;
                                    }
                                    else if (input.Value is string && ((string)input.Value).Length > 1 && ((string)input.Value).First() == '\\')
                                    {
                                        var escapedCommand = ((string)input.Value);
                                        //The $ is escaped
                                        realAssignedValues.Add(input.Key, ((string)input.Value).Substring(1, escapedCommand.Length - 1));
                                        convertedInputs.Add(input.Key, input.Value);
                                        inputsUpdated = true;
                                    }
                                    else
                                    {
                                        realAssignedValues.Add(input.Key, input.Value);
                                        convertedInputs.Add(input.Key, input.Value);
                                    }
                                }
                                else
                                {
                                    realAssignedValues.Add(input.Key, input.Value);
                                    convertedInputs.Add(input.Key, input.Value);
                                }
                            }

                            //If a update was detected then add it to the journal updates
                            if (inputsUpdated)
                            {
                                unassignedStep.UpdateJournal(new Domain.Entities.JournalEntries.JournalEntry()
                                {
                                    CreatedBy = SystemUsers.QUEUE_MANAGER,
                                    CreatedOn = DateTime.UtcNow,
                                    Updates = new List<Update>()
                                    {
                                       new Update()
                                        {
                                            Type = UpdateType.Override,
                                            FieldName = "status",
                                            Value = StepStatuses.Assigned
                                       },
                                        new Update()
                                            {
                                                FieldName = "inputs",
                                                Type = UpdateType.Override,
                                                Value = convertedInputs
                                            }}
                                });
                            }
                            else
                            {
                                unassignedStep.UpdateJournal(new Domain.Entities.JournalEntries.JournalEntry()
                                {
                                    CreatedBy = SystemUsers.QUEUE_MANAGER,
                                    CreatedOn = DateTime.UtcNow,
                                    Updates = new List<Update>()
                                    {
                                       new Update()
                                        {
                                            Type = UpdateType.Override,
                                            FieldName = "status",
                                            Value = StepStatuses.Assigned
                                       }
                                    }
                                });
                            }

                            await _stepsRepository.UpdateStep(unassignedStep);
                            if (inputsUpdated)
                            {
                                //Update the record with real values, this is not commited to DB
                                unassignedStep.UpdateJournal(new Domain.Entities.JournalEntries.JournalEntry()
                                {
                                    CreatedBy = SystemUsers.QUEUE_MANAGER,
                                    CreatedOn = DateTime.UtcNow,
                                    Updates = new List<Update>()
                                    {
                                        new Update()
                                            {
                                                FieldName = "inputs",
                                                Type = UpdateType.Override,
                                                Value = realAssignedValues
                                            }}
                                });
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            //throw e;
                        }
                        assignedStepSuccessfully = true;
                    }
                    else
                    {
                        assignedStepSuccessfully = true;
                    }
                }
                while (!assignedStepSuccessfully);

                if (unassignedStep != null)
                {
                    var template = await _stepTemplateRepository.GetStepTemplateAsync(unassignedStep.StepTemplateId);
                    var botkey = await _botKeysRepository.GetBotKeyAsync(request.BotId);

                    //Decrypt the step
                    unassignedStep.Inputs = DynamicDataUtility.DecryptDynamicData( template.InputDefinitions, unassignedStep.Inputs, EncryptionProtocol.AES256, ClusterStateService.GetEncryptionKey());

                    unassignedStep.RemoveDelimiters();

                    //Encrypt the step
                    unassignedStep.Inputs = DynamicDataUtility.EncryptDynamicData(template.InputDefinitions, unassignedStep.Inputs, EncryptionProtocol.RSA, botkey.PublicEncryptionKey, true);
                }

                stopwatch.Stop();
                return new CommandResult<Step>()
                {
                    ObjectRefId = unassignedStep != null ? unassignedStep.Id.ToString() : "",
                    ElapsedMs = stopwatch.ElapsedMilliseconds,
                    Type = CommandResultTypes.Update,
                    Result = unassignedStep != null ? unassignedStep : null
                };
            }
            else
            {
                return new CommandResult<Step>()
                {
                    ObjectRefId = "",
                    ElapsedMs = stopwatch.ElapsedMilliseconds,
                    Type = CommandResultTypes.None
                };
            }

        }
    }
}
