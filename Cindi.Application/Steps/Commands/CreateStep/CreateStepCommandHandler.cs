
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.StepTemplates;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;


using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Steps.Commands.CreateStep
{
    public class CreateStepCommandHandler : IRequestHandler<CreateStepCommand, CommandResult<Step>>
    {
        private readonly IStateMachine _stateMachine;
        private readonly IEntitiesRepository _entitiesRepository;
        public CreateStepCommandHandler(
            IStateMachine stateMachine,
            IEntitiesRepository entitiesRepository)
        {
            _entitiesRepository = entitiesRepository;
            _stateMachine = stateMachine;
        }

        public async Task<CommandResult<Step>> Handle(CreateStepCommand request, CancellationToken cancellationToken)
        {
            //var stopwatch = new Stopwatch();
            //stopwatch.Start();
            var startDate = DateTime.Now;

            var resolvedTemplate = await _entitiesRepository.GetFirstOrDefaultAsync<StepTemplate>(st => st.ReferenceId == request.StepTemplateId);

            if (resolvedTemplate == null)
            {
                throw new StepTemplateNotFoundException("Step template " + request.StepTemplateId + " not found.");
            }

            var newStep = resolvedTemplate.GenerateStep(request.StepTemplateId,
                request.CreatedBy,
                request.Name, request.Description,
                request.Inputs,
                request.Tests, request.WorkflowId,
                _stateMachine.EncryptionKey,
                request.ExecutionTemplateId,
                request.ExecutionScheduleId);

            await _entitiesRepository.Insert(newStep);


            //stopwatch.Stop();
            return new CommandResult<Step>()
            {
                ObjectRefId = newStep.Id.ToString(),
                ElapsedMs = Convert.ToInt64((DateTime.Now - startDate).TotalMilliseconds),//stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Create,
                Result = newStep
            };
        }
    }
}
