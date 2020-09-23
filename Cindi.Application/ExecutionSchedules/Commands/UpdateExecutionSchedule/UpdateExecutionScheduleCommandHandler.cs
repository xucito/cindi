
using Cindi.Application.Exceptions;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterOperation;
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

namespace Cindi.Application.ExecutionSchedules.Commands.UpdateExecutionSchedule
{
    public class UpdateExecutionScheduleCommandHandler : IRequestHandler<UpdateExecutionScheduleCommand, CommandResult>
    {
        private readonly IClusterStateService _clusterStateService;
        private readonly IClusterService _clusterService;

        public UpdateExecutionScheduleCommandHandler(
            IClusterStateService service,
            IClusterService clusterService
            )
        {
            _clusterStateService = service;
            _clusterService = clusterService;
        }

        public async Task<CommandResult> Handle(UpdateExecutionScheduleCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            ExecutionSchedule schedule = await _clusterService.GetFirstOrDefaultAsync<ExecutionSchedule>(st => st.Name == request.Name);

            if (schedule == null)
            {
                throw new InvalidExecutionScheduleException("Execution Schedule with name " + request.Name + " is invalid.");
            }


            if (request.Schedule != null)
                foreach (var scheduleString in request.Schedule)
                {
                    var isValid = SchedulerUtility.IsValidScheduleString(scheduleString);
                    if (!isValid)
                    {
                        throw new InvalidExecutionScheduleException("Schedule " + scheduleString + " is invalid.");
                    }
                }

            var executionScheduleLock = await _clusterService.Handle(new RequestDataShard()
            {
                Type = schedule.ShardType,
                ObjectId = schedule.Id,
                CreateLock = true
            });


            ExecutionSchedule existingValue;

            

            if (executionScheduleLock.IsSuccessful && executionScheduleLock.AppliedLocked)
            {
                existingValue = (ExecutionSchedule)executionScheduleLock.Data;

                List<Update> updates = new List<Update>();

                if (request.IsDisabled != null && schedule.IsDisabled != request.IsDisabled)
                {
                    existingValue.IsDisabled = request.IsDisabled.Value;
                }

                if (request.Schedule != null && schedule.Schedule != request.Schedule)
                {
                    existingValue.NextRun = SchedulerUtility.NextOccurence(request.Schedule, DateTime.UtcNow);

                    existingValue.Schedule = request.Schedule;
                }

                if (request.Description != null && schedule.Description != request.Description)
                {
                    existingValue.Description = request.Description;
                }


                var result = await _clusterService.AddWriteOperation(new EntityWriteOperation<ExecutionSchedule>()
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
