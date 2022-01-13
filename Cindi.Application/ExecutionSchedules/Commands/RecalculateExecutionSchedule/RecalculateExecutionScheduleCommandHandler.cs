using Cindi.Application.Exceptions;
using Cindi.Application.ExecutionTemplates.Commands.ExecuteExecutionTemplate;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Utilities;
using Cindi.Domain.Entities.ExecutionSchedule;
using Cindi.Domain.ValueObjects;
using Cindi.Persistence.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        private readonly IClusterStateService _clusterStateService;
        private readonly ApplicationDbContext _context;
        private IMediator _mediator;

        public UpdateExecutionScheduleCommandHandler(
            IClusterStateService service,
            ApplicationDbContext context,
            IMediator mediator)
        {
            _clusterStateService = service;
            _context = context;
            _mediator = mediator;
        }

        public async Task<CommandResult> Handle(RecalculateExecutionScheduleCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            ExecutionSchedule schedule = await _context.ExecutionSchedules.FirstOrDefaultAsync(st => st.Name == request.Name);

            if (schedule == null)
            {
                throw new InvalidExecutionScheduleException("Execution Schedule with name " + request.Name + " is invalid.");
            }

            var appliedLock = await _context.LockObject(schedule);


            ExecutionSchedule existingValue;

            if (appliedLock != null)
            {
                existingValue = await _context.ExecutionSchedules.FirstOrDefaultAsync(st => st.Name == request.Name);
                existingValue.NextRun = SchedulerUtility.NextOccurence(existingValue.Schedule, DateTime.UtcNow);
                existingValue.Unlock();
                _context.Update(existingValue);
                await _context.SaveChangesAsync();
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
