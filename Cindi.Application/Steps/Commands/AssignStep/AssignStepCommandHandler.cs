using Cindi.Application.Entities.Command.CreateTrackedEntity;
using Cindi.Application.Exceptions;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.RPCs;
using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Domain.SystemCommands;
using ConsensusCore.Node;
using ConsensusCore.Node.Communication.Controllers;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IEntitiesRepository _entitiesRepository;
        private readonly IClusterStateService _clusterStateService;
        public ILogger<AssignStepCommandHandler> Logger;
        private readonly IClusterRequestHandler _node;
        private IMemoryCache _cache;
        private IMediator _mediator;

        public AssignStepCommandHandler(
            IEntitiesRepository entitiesRepository,
            IClusterStateService stateService,
            ILogger<AssignStepCommandHandler> logger,
            IClusterRequestHandler node,
            IMemoryCache cache,
            IMediator mediator
            )
        {
            _entitiesRepository = entitiesRepository;
            _clusterStateService = stateService;
            Logger = logger;
            _node = node;
            _cache = cache;
            _mediator = mediator;
        }
        public async Task<CommandResult<Step>> Handle(AssignStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            List<Guid> ignoreUnassignedSteps = new List<Guid>();
            if (_clusterStateService.GetSettings.AssignmentEnabled)
            {
                var assignedStepSuccessfully = false;
                Step unassignedStep = null;
                var dateChecked = DateTime.UtcNow;
                BotKey botkey;

                if (!_cache.TryGetValue(request.BotId, out botkey))
                {
                    // Set cache options.
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        // Keep in cache for this time, reset time if accessed.
                        .SetSlidingExpiration(TimeSpan.FromSeconds(10));
                    botkey = await _entitiesRepository.GetFirstOrDefaultAsync<BotKey>(bk => bk.Id == request.BotId);
                    // Save data in cache.
                    _cache.Set(request.BotId, botkey, cacheEntryOptions);
                }

                if (botkey.IsDisabled)
                {
                    return new CommandResult<Step>(new BotKeyAssignmentException("Bot " + botkey.Id + " is disabled."))
                    {
                        Type = CommandResultTypes.Update,
                        ElapsedMs = stopwatch.ElapsedMilliseconds
                    };
                }

                ignoreUnassignedSteps.AddRange(_clusterStateService.GetState().Locks.Where(l => l.Key.Contains("_object")).Select(ol => new Guid(ol.Key.Split(':').Last())));

                do
                {
                    unassignedStep = (await _entitiesRepository.GetAsync<Step>(s => s.Status == StepStatuses.Unassigned && request.StepTemplateIds.Contains(s.StepTemplateId) && !ignoreUnassignedSteps.Contains(s.Id), null, "CreatedOn:1", 1, 0)).FirstOrDefault();
                    if (unassignedStep != null)
                    {
                        var assigned = await _node.Handle(new RequestDataShard()
                        {
                            Type = unassignedStep.ShardType,
                            ObjectId = unassignedStep.Id,
                            CreateLock = true,
                            LockTimeoutMs = 10000
                        });
                        //Apply a lock on the item
                        if (assigned != null && assigned.IsSuccessful && assigned.AppliedLocked)
                        {

                            //Real values to pass to the Microservice
                            Dictionary<string, object> realAssignedValues = new Dictionary<string, object>();
                            //Inputs that have been converted to reference expression
                            Dictionary<string, object> convertedInputs = new Dictionary<string, object>();

                            var template = await _entitiesRepository.GetFirstOrDefaultAsync<StepTemplate>(st => st.ReferenceId == unassignedStep.StepTemplateId);
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
                                                var foundGlobalValue = await _entitiesRepository.GetFirstOrDefaultAsync<GlobalValue>(gv => gv.Name == convertedValue);
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
                                                    convertedInputs.Add(input.Key, input.Value + ":" + foundGlobalValue.Version);
                                                }
                                            }
                                            //copy by value
                                            else
                                            {
                                                var foundGlobalValue = await _entitiesRepository.GetFirstOrDefaultAsync<GlobalValue>(gv => gv.Name == convertedValue);
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
                                    unassignedStep.Status = StepStatuses.Assigned;

                                    unassignedStep.Inputs = convertedInputs;

                                    unassignedStep.AssignedTo = request.BotId;
                                }
                                else
                                {
                                    unassignedStep.Status = StepStatuses.Assigned;
                                }

                                await _mediator.Send(new WriteEntityCommand<Step>()
                                {
                                    Data = unassignedStep,
                                    WaitForSafeWrite = true,
                                    Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Update,
                                    RemoveLock = true,
                                    LockId = assigned.LockId.Value,
                                    User = SystemUsers.QUEUE_MANAGER
                                });

                                //await _entitiesRepository.UpdateStep(unassignedStep);
                                if (inputsUpdated)
                                {
                                    //Update the record with real values, this is not commited to DB
                                    unassignedStep.Inputs = realAssignedValues;
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
                            ignoreUnassignedSteps.Add(unassignedStep.Id);
                            assignedStepSuccessfully = false;
                        }
                    }
                    //There were no unassigned steps to assign
                    else
                    {
                        assignedStepSuccessfully = true;
                    }
                }
                while (!assignedStepSuccessfully);


                if (unassignedStep != null)
                {
                    var template = await _entitiesRepository.GetFirstOrDefaultAsync<StepTemplate>(st => st.ReferenceId == unassignedStep.StepTemplateId);

                    //Decrypt the step
                    unassignedStep.Inputs = DynamicDataUtility.DecryptDynamicData(template.InputDefinitions, unassignedStep.Inputs, EncryptionProtocol.AES256, ClusterStateService.GetEncryptionKey());

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
