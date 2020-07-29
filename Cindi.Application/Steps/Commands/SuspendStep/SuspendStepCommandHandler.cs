using Cindi.Application.Interfaces;
using Cindi.Application.Options;
using Cindi.Application.Results;
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

namespace Cindi.Application.Steps.Commands.SuspendStep
{
    public class SuspendStepCommandHandler : IRequestHandler<SuspendStepCommand, CommandResult>
    {
        public IEntitiesRepository _entitiesRepository;
        public ILogger<SuspendStepCommandHandler> Logger;
        private CindiClusterOptions _option;
        private readonly IClusterRequestHandler _node;

        public SuspendStepCommandHandler(IEntitiesRepository entitiesRepository,
            ILogger<SuspendStepCommandHandler> logger,
            IOptionsMonitor<CindiClusterOptions> options,
             IClusterRequestHandler node)
        {
            _entitiesRepository = entitiesRepository;
            _node = node;
            Logger = logger;
            options.OnChange((change) =>
            {
                _option = change;
            });
        }


        public async Task<CommandResult> Handle(SuspendStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Step step = await _entitiesRepository.GetFirstOrDefaultAsync<Step>(e => e.Id == request.StepId);

            if(step == null)
            {
                throw new Exception("Failed to find step " + step.Id); 
            }

            if (step.Status == StepStatuses.Suspended)
            {
                return new CommandResult()
                {
                    ElapsedMs = stopwatch.ElapsedMilliseconds,
                    ObjectRefId = request.StepId.ToString(),
                    Type = CommandResultTypes.None
                };
            }

            var stepLockResult = await _node.Handle(new RequestDataShard()
            {
                Type = "Step",
                ObjectId = request.StepId,
                CreateLock = true,
                LockTimeoutMs = 10000
            });

            // Applied the lock successfully
            if (stepLockResult.IsSuccessful && stepLockResult.AppliedLocked)
            {
                Logger.LogInformation("Applied lock on step " + request.StepId + " with lock id " + stepLockResult.LockId);
                step = (Step)stepLockResult.Data;
                if (step.Status == StepStatuses.Unassigned ||
                    step.Status == StepStatuses.Suspended ||
                    step.Status == StepStatuses.Assigned // You should only be suspending a assigned step if it is being suspended by the bot assigned, check to be done in presentation
                    )
                {
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
                                Value = StepStatuses.Suspended
                            },
                            new Update()
                            {
                                Type = UpdateType.Override,
                                FieldName = "suspendeduntil",
                                Value = request.SuspendedUntil
                            },
                            new Update()
                            {
                                FieldName = "assignedto",
                                Type = UpdateType.Override,
                                Value = null
                            }

                        }
                    });

                    var result = await _node.Handle(new AddShardWriteOperation()
                    {
                        Data = step,
                        WaitForSafeWrite = true,
                        Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Update,
                        RemoveLock = true,
                        LockId = stepLockResult.LockId.Value
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
                    throw new InvalidStepStatusException("Step " + request.StepId + " could not be paused as it has status " + step.Status);
                }
            }
            else
            {
                throw new FailedClusterOperationException("Step " + request.StepId + " failed to have a lock applied.");
            }
        }
    }
}
