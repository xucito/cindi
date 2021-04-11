using Cindi.Application.Exceptions;
using Cindi.Application.ExecutionTemplates.Commands.ExecuteExecutionTemplate;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Utilities;
using Cindi.Domain.Entities.ExecutionSchedule;
using Cindi.Domain.Entities.ExecutionTemplates;


using Cronos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.ExecutionSchedules.Commands.CreateExecutionSchedule
{
    public class CreateExecutionScheduleCommandHandler : IRequestHandler<CreateExecutionScheduleCommand, CommandResult>
    {
        private readonly IEntitiesRepository _entitiesRepository;
        private readonly IStateMachine _stateMachine;
        private IMediator _mediator;

        public CreateExecutionScheduleCommandHandler(
            IEntitiesRepository entitiesRepository,
            IStateMachine stateMachine,
            IMediator mediator)
        {
            _entitiesRepository = entitiesRepository;
            _stateMachine = stateMachine;
            _mediator = mediator;
        }

        public async Task<CommandResult> Handle(CreateExecutionScheduleCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            ExecutionSchedule schedule = await _entitiesRepository.GetFirstOrDefaultAsync<ExecutionSchedule>(st => st.Name == request.Name);

            if (schedule != null)
            {
                throw new InvalidExecutionScheduleException("Execution Schedule with name " + request.Name + " is invalid.");
            }

            ExecutionTemplate template = await _entitiesRepository.GetFirstOrDefaultAsync<ExecutionTemplate>(st => st.Name == request.ExecutionTemplateName);

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
                Id = Guid.NewGuid(),
                Name = request.Name,
                ExecutionTemplateName = request.ExecutionTemplateName,
                Description = request.Description,
                Schedule = request.Schedule,
                NextRun = SchedulerUtility.NextOccurence(request.Schedule),
                EnableConcurrent = request.EnableConcurrent,
                TimeoutMs = request.TimeoutMs
            };

            await _entitiesRepository.Insert(executionSchedule);

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
