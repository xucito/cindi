
using Cindi.Application.Exceptions;
using Cindi.Application.ExecutionTemplates.Commands.ExecuteExecutionTemplate;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Utilities;
using Cindi.Domain.Entities.ExecutionSchedule;
using Cindi.Domain.Enums;
using Cindi.Domain.ValueObjects;
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
        private readonly IStateMachine _stateMachine;

        public UpdateExecutionScheduleCommandHandler(
            IEntitiesRepository entitiesRepository,
            IStateMachine stateMachine)
        {
            _entitiesRepository = entitiesRepository;
            _stateMachine = stateMachine;
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

            schedule.NextRun = SchedulerUtility.NextOccurence(schedule.Schedule, DateTime.UtcNow);

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
