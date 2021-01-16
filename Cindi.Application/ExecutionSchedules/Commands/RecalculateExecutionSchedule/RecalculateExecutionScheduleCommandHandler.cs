
using Cindi.Application.Exceptions;
using Cindi.Application.ExecutionTemplates.Commands.ExecuteExecutionTemplate;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterOperation;
using Cindi.Application.Utilities;
using Cindi.Domain.Entities.ExecutionSchedule;
using Cindi.Domain.Enums;
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
        private readonly IClusterService _clusterService;
        private readonly IStateMachine _stateMachine;

        public UpdateExecutionScheduleCommandHandler(IEntitiesRepository entitiesRepository,
            IClusterStateService service,
            IClusterService clusterService)
        {
            _clusterService = clusterService;
            _stateMachine = service;
        }

        public async Task<CommandResult> Handle(RecalculateExecutionScheduleCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            ExecutionSchedule schedule = await _clusterService.GetFirstOrDefaultAsync<ExecutionSchedule>(st => st.Name == request.Name);

            if (schedule == null)
            {
                throw new InvalidExecutionScheduleException("Execution Schedule with name " + request.Name + " is invalid.");
            }

            var executionScheduleLock = await _clusterService.Handle(new RequestDataShard()
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
                existingValue.NextRun = SchedulerUtility.NextOccurence(existingValue.Schedule, DateTime.UtcNow);

                await _clusterService.AddWriteOperation(new EntityWriteOperation<ExecutionSchedule>()
                {
                    Data = existingValue,
                    Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Update,
                    User = SystemUsers.SCHEDULE_MANAGER,
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
