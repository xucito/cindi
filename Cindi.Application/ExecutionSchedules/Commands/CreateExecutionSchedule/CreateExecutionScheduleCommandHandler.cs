using Cindi.Application.Exceptions;
using Cindi.Application.ExecutionTemplates.Commands.ExecuteExecutionTemplate;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Utilities;
using Cindi.Domain.Entities.ExecutionSchedule;
using Cindi.Domain.Entities.ExecutionTemplates;
using Nest;
using Cronos;
using MediatR;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cindi.Application.Entities.Queries.GetEntity;

namespace Cindi.Application.ExecutionSchedules.Commands.CreateExecutionSchedule
{
    public class CreateExecutionScheduleCommandHandler : IRequestHandler<CreateExecutionScheduleCommand, CommandResult>
    {
        private readonly IClusterStateService _clusterStateService;
        private readonly ElasticClient _context;
        private IMediator _mediator;

        public CreateExecutionScheduleCommandHandler(
            IClusterStateService service,
            ElasticClient context,
            IMediator mediator)
        {
            _clusterStateService = service;
            _context = context;
            _mediator = mediator;
        }

        public async Task<CommandResult> Handle(CreateExecutionScheduleCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            ExecutionSchedule schedule = (await _mediator.Send(new GetEntityQuery<ExecutionSchedule>() { 
                Expression = (e => e.Query(q => q.Term(f => f.Name, request.Name)
                    ))
            })).Result;

            if (schedule != null)
            {
                throw new InvalidExecutionScheduleException("Execution Schedule with name " + request.Name + " is invalid.");
            }

            ExecutionTemplate template = (await _mediator.Send(new GetEntityQuery<ExecutionTemplate>()
            {
                Expression = (e => e.Query(q => q.Term(f => f.Name, request.ExecutionTemplateName)
                    ))
            })).Result;

            if (template == null)
            {
                throw new InvalidExecutionScheduleException("Execution Template with name " + request.ExecutionTemplateName + " is invalid.");
            }

            foreach (var scheduleString in request.Schedule)
            {
                var isValid = SchedulerUtility.IsValidScheduleString(scheduleString);
                if (!isValid)
                {
                    throw new InvalidExecutionScheduleException("Schedule " + scheduleString + " is invalid.");
                }
            }

            var executionSchedule = new ExecutionSchedule()
            {
                Name = request.Name,
                ExecutionTemplateName = request.ExecutionTemplateName,
                Description = request.Description,
                CreatedBy = request.CreatedBy,
                Schedule = request.Schedule,
                NextRun = SchedulerUtility.NextOccurence(request.Schedule)
            };

            await _context.IndexDocumentAsync(executionSchedule);
            

            if (request.RunImmediately)
            {
                await _mediator.Send(new ExecuteExecutionTemplateCommand()
                {
                    CreatedBy = request.CreatedBy,
                    ExecutionScheduleId = executionSchedule.Id,
                    Name = executionSchedule.ExecutionTemplateName
                });
            }


            stopwatch.Stop();
            return new CommandResult<ExecutionSchedule>()
            {
                ObjectRefId = executionSchedule.Id.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Create,
                Result = executionSchedule
            };
        }
    }
}
