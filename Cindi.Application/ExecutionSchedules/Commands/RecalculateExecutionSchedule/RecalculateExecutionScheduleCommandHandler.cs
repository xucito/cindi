using Cindi.Application.Exceptions;
using Cindi.Application.ExecutionTemplates.Commands.ExecuteExecutionTemplate;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Utilities;
using Cindi.Domain.Entities.ExecutionSchedule;
using Cindi.Domain.ValueObjects;
using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Node.Communication.Controllers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.ExecutionSchedules.Commands.RecalculateExecutionSchedule
{
    public class UpdateExecutionScheduleCommandHandler : IRequestHandler<RecalculateExecutionScheduleCommand, CommandResult>
    {
        private readonly IEntitiesRepository _entitiesRepository;
        private readonly IClusterStateService _clusterStateService;
        private readonly IClusterRequestHandler _node;
        private IMediator _mediator;

        public UpdateExecutionScheduleCommandHandler(IEntitiesRepository entitiesRepository,
            IClusterStateService service,
            IClusterRequestHandler node,
            IMediator mediator)
        {
            _entitiesRepository = entitiesRepository;
            _clusterStateService = service;
            _node = node;
            _mediator = mediator;
        }

        public async Task<CommandResult> Handle(RecalculateExecutionScheduleCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            ExecutionSchedule schedule = await _entitiesRepository.GetFirstOrDefaultAsync<ExecutionSchedule>(st => st.Name == request.Name);

            if (schedule == null)
            {
                throw new InvalidExecutionScheduleException("Execution Schedule with name " + request.Name + " is invalid.");
            }

            var executionScheduleLock = await _node.Handle(new RequestDataShard()
            {
                Type = schedule.ShardType,
                ObjectId = schedule.Id,
                CreateLock = true,
                LockTimeoutMs = 10000
            });


            ExecutionSchedule existingValue;

            if (executionScheduleLock.IsSuccessful && executionScheduleLock.AppliedLocked)
            {
                existingValue = (ExecutionSchedule)executionScheduleLock.Data;
                existingValue.UpdateJournal(new Domain.Entities.JournalEntries.JournalEntry()
                {
                    CreatedOn = DateTime.UtcNow,
                    Updates = new List<Domain.ValueObjects.Update>()
                        {
                            new Update()
                            {
                                Type = UpdateType.Override,
                                FieldName = "nextrun",
                                Value = SchedulerUtility.NextOccurence(existingValue.Schedule, DateTime.UtcNow)
                            }
                        }
                });

                var result = await _node.Handle(new AddShardWriteOperation()
                {
                    Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Update,
                    WaitForSafeWrite = true,
                    Data = existingValue,
                    RemoveLock = true
                });
            }

            stopwatch.Stop();
            return new CommandResult<ExecutionSchedule>()
            {
                ObjectRefId = schedule.Id.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Update,
                Result = schedule
            };
        }
    }
}
