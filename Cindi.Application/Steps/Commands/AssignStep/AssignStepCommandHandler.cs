using Cindi.Application.Exceptions;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using Nest;
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
using Cindi.Application.Utilities;
using Elasticsearch.Net;

namespace Cindi.Application.Steps.Commands.AssignStep
{
    public class AssignStepCommandHandler : IRequestHandler<AssignStepCommand, EncryptedCommandResult<Step>>
    {
        private readonly IClusterStateService _clusterStateService;
        public ILogger<AssignStepCommandHandler> Logger;
        private readonly ElasticClient _context;
        private IMemoryCache _cache;

        public AssignStepCommandHandler(
            IClusterStateService stateService,
            ILogger<AssignStepCommandHandler> logger,
            ElasticClient context,
            IMemoryCache cache
            )
        {
            _clusterStateService = stateService;
            Logger = logger;
            _context = context;
            _cache = cache;
        }
        public async Task<EncryptedCommandResult<Step>> Handle(AssignStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            List<Guid> ignoreUnassignedSteps = new List<Guid>();
            string encryptedEncryptionKey = null;
            if (_clusterStateService.GetSettings.AssignmentEnabled)
            {
                var assignedStepSuccessfully = false;
                Step unassignedStep = null;
                List<Step> possibleSteps = null;
                var dateChecked = DateTime.UtcNow;
                BotKey botkey;

                if (!_cache.TryGetValue(request.BotId, out botkey))
                {
                    // Set cache options.
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        // Keep in cache for this time, reset time if accessed.
                        .SetSlidingExpiration(TimeSpan.FromSeconds(10));
                    botkey = await _context.FirstOrDefaultAsync<BotKey>(request.BotId);
                    // Save data in cache.
                    _cache.Set(request.BotId, botkey, cacheEntryOptions);
                }

                if (botkey.IsDisabled)
                {
                    return new EncryptedCommandResult<Step>(new BotKeyAssignmentException("Bot " + botkey.Id + " is disabled."))
                    {
                        Type = CommandResultTypes.Update,
                        ElapsedMs = stopwatch.ElapsedMilliseconds
                    };
                }

                
                possibleSteps = (await _context.SearchAsync<Step>(s =>
                s.Query(q =>
                    q.Term(o => o.Status.Suffix("keyword"), StepStatuses.Unassigned) &&
                    q.Terms(t => t.Field(o => o.StepTemplateId.Suffix("keyword")).Terms(request.StepTemplateIds)) &&
                    !q.Terms(t => t.Field(o => o.Id.Suffix("keyword")).Terms(ignoreUnassignedSteps))).Size(10).Sort(a => a.Ascending(f => f.CreatedOn)))).Hits.Select(h => h.Source).ToList();

                foreach(var stepToLock in possibleSteps)
                { 
                    var lockId = await _context.LockObject(stepToLock);
                    //Apply a lock on the item
                    if (lockId != null)
                    {
                        unassignedStep = stepToLock;
                        //Real values to pass to the Microservice
                        Dictionary<string, object> realAssignedValues = new Dictionary<string, object>();
                        //Inputs that have been converted to reference expression
                        Dictionary<string, object> convertedInputs = new Dictionary<string, object>();

                        var template = await _context.FirstOrDefaultAsync<StepTemplate>(s => s.Query(q => q.Term(o => o.Field(f => f.ReferenceId.Suffix("keyword")).Value(unassignedStep.StepTemplateId))));
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
                                            var foundGlobalValue = await _context.FirstOrDefaultAsync<GlobalValue>(s => s.Query(q => q.Term(o => o.Field(f => f.Name.Suffix("keyword")).Value(convertedValue))).Sort(so => so.Descending(o => o.Version)));
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
                                            var foundGlobalValue = await _context.FirstOrDefaultAsync<GlobalValue>(s => s.Query(q => q.Term(o => o.Field(f => f.Name.Suffix("keyword")).Value(convertedValue))).Sort(so => so.Descending(o => o.Version)));
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
                            unassignedStep.Status = StepStatuses.Assigned;
                            if (inputsUpdated)
                            {
                                unassignedStep.Inputs = convertedInputs;
                                unassignedStep.AssignedTo = request.BotId;
                            }
                            await _context.IndexDocumentAsync(unassignedStep);
                            await _context.Unlock<Step>(unassignedStep.Id);

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
                        break;
                    }
                    else
                    {
                        Logger.LogWarning("Failed to lock " + stepToLock.Id);
                        assignedStepSuccessfully = false;
                    }
                }


                if (unassignedStep != null)
                {
                    var template = await _context.FirstOrDefaultAsync<StepTemplate>(s => s.Query(q => q.Term(o => o.Field(f => f.ReferenceId.Suffix("keyword")).Value(unassignedStep.StepTemplateId))));

                    //Decrypt the step
                    unassignedStep.Inputs = DynamicDataUtility.DecryptDynamicData(template.InputDefinitions, unassignedStep.Inputs, EncryptionProtocol.AES256, ClusterStateService.GetEncryptionKey());

                    unassignedStep.RemoveDelimiters();
                    //Generate a random encryption key
                    var encryptionKey = SecurityUtility.RandomString(32);
                    //Generate a random encryption key
                    //Encrypt the step
                    unassignedStep.Inputs = DynamicDataUtility.EncryptDynamicData(template.InputDefinitions, unassignedStep.Inputs, EncryptionProtocol.AES256, encryptionKey, true);

                    encryptedEncryptionKey = SecurityUtility.RsaEncryptWithPublic(encryptionKey, botkey.PublicEncryptionKey);
                }

                stopwatch.Stop();

                return new EncryptedCommandResult<Step>()
                {
                    ObjectRefId = unassignedStep != null ? unassignedStep.Id.ToString() : "",
                    ElapsedMs = stopwatch.ElapsedMilliseconds,
                    Type = CommandResultTypes.Update,
                    Result = unassignedStep != null ? unassignedStep : null,
                    EncryptionKey = encryptedEncryptionKey
                };
            }
            else
            {
                return new EncryptedCommandResult<Step>()
                {
                    ObjectRefId = "",
                    ElapsedMs = stopwatch.ElapsedMilliseconds,
                    Type = CommandResultTypes.None
                };
            }

        }
    }
}
