using Cindi.Application.Exceptions;
using Cindi.Application.ExecutionTemplates.Commands.ExecuteExecutionTemplate;
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

namespace Cindi.Application.ExecutionSchedules.Commands.RecalculateExecutionSchedule
{
    public class UpdateExecutionScheduleCommandHandler : IRequestHandler<RecalculateExecutionScheduleCommand, CommandResult>
    {
        private readonly IClusterStateService _clusterStateService;
        private IMediator _mediator;
        ElasticClient _context;


        public UpdateExecutionScheduleCommandHandler(
            IClusterStateService service,
            IMediator mediator,
            ElasticClient context)
        {
            _clusterStateService = service;
            _mediator = mediator;
            _context = context;
        }

        public async Task<CommandResult> Handle(RecalculateExecutionScheduleCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            ExecutionSchedule schedule = (await _mediator.Send(new GetEntityQuery<ExecutionSchedule>()
            {
                Expression = (e => e.Query(q => q.Term(f => f.Name, request.Name)))
            })).Result;

            if (schedule == null)
            {
                throw new InvalidExecutionScheduleException("Execution Schedule with name " + request.Name + " is invalid.");
            }

            var appliedLock = await _context.LockObject(schedule);

            ExecutionSchedule existingValue;

            if (appliedLock != null)
            {
                existingValue = (await _mediator.Send(new GetEntityQuery<ExecutionSchedule>()
                {
                    Expression = (e => e.Query(q => q.Term(f => f.Name, request.Name)))
                })).Result;
                existingValue.NextRun = SchedulerUtility.NextOccurence(existingValue.Schedule, DateTime.UtcNow);
                existingValue.Unlock();
                await _context.IndexDocumentAsync(existingValue);
                
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
