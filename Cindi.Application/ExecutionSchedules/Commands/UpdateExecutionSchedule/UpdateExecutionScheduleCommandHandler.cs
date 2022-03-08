using Cindi.Application.Exceptions;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Utilities;
using Cindi.Domain.Entities.ExecutionSchedule;
using Cindi.Domain.ValueObjects;
using Nest;
using MediatR;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cindi.Application.Entities.Queries.GetEntity;

namespace Cindi.Application.ExecutionSchedules.Commands.UpdateExecutionSchedule
{
    public class UpdateExecutionScheduleCommandHandler : IRequestHandler<UpdateExecutionScheduleCommand, CommandResult>
    {

        private readonly IClusterStateService _clusterStateService;
        private readonly ElasticClient _context;
        private IMediator _mediator;

        public UpdateExecutionScheduleCommandHandler(
            IClusterStateService service,
            ElasticClient context,
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

            ExecutionSchedule schedule = (await _mediator.Send(new GetEntityQuery<ExecutionSchedule>()
            {
                Expression = (e => e.Query(q => q.Term(f => f.Name, request.Name)
                    ))
            })).Result;

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

            var lockId = await _context.LockObject(schedule);

            if (lockId != null)
            {
                if (request.IsDisabled != null && schedule.IsDisabled != request.IsDisabled)
                {
                    schedule.IsDisabled = request.IsDisabled.Value;
                }

                if (request.Schedule != null && schedule.Schedule != request.Schedule)
                {
                    schedule.Schedule = request.Schedule;
                }

                if (request.Description != null && schedule.Description != request.Description)
                {
                    schedule.Description = request.Description;
                }

                await _context.IndexDocumentAsync(schedule);
                
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
