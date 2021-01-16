
using Cindi.Application.Exceptions;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Utilities;
using Cindi.Domain.Entities.ExecutionSchedule;
using Cindi.Domain.ValueObjects;
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
        private readonly IStateMachine _stateMachine;
        private readonly IEntitiesRepository _entitiesRepository;

        public UpdateExecutionScheduleCommandHandler(
            IStateMachine stateMachine,
            IEntitiesRepository entitiesRepository
            )
        {
            _stateMachine = stateMachine;
            _entitiesRepository = entitiesRepository;
        }

        public async Task<CommandResult> Handle(UpdateExecutionScheduleCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            ExecutionSchedule schedule = await _entitiesRepository.GetFirstOrDefaultAsync<ExecutionSchedule>(st => st.Name == request.Name);

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

            List<Update> updates = new List<Update>();

            if (request.IsDisabled != null && schedule.IsDisabled != request.IsDisabled)
            {
                schedule.IsDisabled = request.IsDisabled.Value;
            }

            if (request.Schedule != null && schedule.Schedule != request.Schedule)
            {
                schedule.NextRun = SchedulerUtility.NextOccurence(request.Schedule, DateTime.UtcNow);

                schedule.Schedule = request.Schedule;
            }

            if (request.Description != null && schedule.Description != request.Description)
            {
                schedule.Description = request.Description;
            }

            await _entitiesRepository.Update(schedule);

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
