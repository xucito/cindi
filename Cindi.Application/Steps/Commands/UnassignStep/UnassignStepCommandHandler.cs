using Cindi.Application.Interfaces;
using Cindi.Application.Options;
using Cindi.Application.Results;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.State;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.ValueObjects;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.RPCs;
using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Node;
using ConsensusCore.Node.Communication.Controllers;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Steps.Commands.UnassignStep
{
    public class UnassignStepCommandHandler : IRequestHandler<UnassignStepCommand, CommandResult>
    {
        public IEntityRepository _entityRepository;
        public ILogger<UnassignStepCommandHandler> Logger;
        private CindiClusterOptions _option;
        private readonly IClusterRequestHandler _node;

        public UnassignStepCommandHandler(IEntityRepository entityRepository,
            ILogger<UnassignStepCommandHandler> logger,
            IOptionsMonitor<CindiClusterOptions> options,
             IClusterRequestHandler node)
        {
            _entityRepository = entityRepository;
            _node = node;
            Logger = logger;
            options.OnChange((change) =>
            {
                _option = change;
            });
        }


        public async Task<CommandResult> Handle(UnassignStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var stepLockResult = await _node.Handle(new RequestDataShard()
            {
                Type = "Step",
                ObjectId = request.StepId,
                CreateLock = true
            });

            if (stepLockResult.IsSuccessful && stepLockResult.AppliedLocked)
            {
                var step = (Step)stepLockResult.Data;
                if (step.Status != StepStatuses.Suspended)
                {
                    Logger.LogWarning("Step " + request.StepId + " has status " + step.Status + ". Only suspended steps can be unassigned.");
                    return new CommandResult()
                    {
                        ElapsedMs = stopwatch.ElapsedMilliseconds,
                        ObjectRefId = step.Id.ToString(),
                        Type = CommandResultTypes.None
                    };
                }

                step.UpdateJournal(new Domain.Entities.JournalEntries.JournalEntry()
                {
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = request.CreatedBy,
                    Updates = new List<Domain.ValueObjects.Update>()
                        {
                            new Update()
                            {
                                Type = UpdateType.Override,
                                FieldName = "status",
                                Value = StepStatuses.Unassigned,
                            }

                        }
                });

                var result =  await _node.Handle(new AddShardWriteOperation()
                {
                    Data = step,
                    WaitForSafeWrite = true,
                    Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Update,
                    RemoveLock = true
                });


                if (result.IsSuccessful)
                {
                    return new CommandResult()
                    {
                        ElapsedMs = stopwatch.ElapsedMilliseconds,
                        ObjectRefId = step.Id.ToString(),
                        Type = CommandResultTypes.Update
                    };
                }
                else
                {
                    throw new FailedClusterOperationException("Failed to apply cluster operation with for step " + step.Id);
                }
            }
            else
            {
                throw new FailedClusterOperationException("Step " + request.StepId + " failed to have a lock applied.");
            }
        }
    }
}
