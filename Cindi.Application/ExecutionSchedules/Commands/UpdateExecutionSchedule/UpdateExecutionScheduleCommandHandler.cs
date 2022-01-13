using Cindi.Application.Exceptions;
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

namespace Cindi.Application.ExecutionSchedules.Commands.UpdateExecutionSchedule
{
    public class UpdateExecutionScheduleCommandHandler : IRequestHandler<UpdateExecutionScheduleCommand, CommandResult>
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

        public async Task<CommandResult> Handle(UpdateExecutionScheduleCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            ExecutionSchedule schedule = await _context.ExecutionSchedules.FirstOrDefaultAsync(st => st.Name == request.Name);

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

            var existingValue = await _context.LockObject(schedule);

            if (existingValue != null)
            {
                if (request.IsDisabled != null && schedule.IsDisabled != request.IsDisabled)
                {
                    existingValue.IsDisabled = request.IsDisabled.Value;
                }

                if (request.Schedule != null && schedule.Schedule != request.Schedule)
                {
                    existingValue.Schedule = request.Schedule;
                }

                if (request.Description != null && schedule.Description != request.Description)
                {
                    existingValue.Description = request.Description;
                }

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
