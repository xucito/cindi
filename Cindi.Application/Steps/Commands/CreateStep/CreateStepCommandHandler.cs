using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.StepTemplates;
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
    public class CreateStepCommandHandler : IRequestHandler<CreateStepCommand, CommandResult>
    {
        private readonly IStepsRepository _stepsRepository;
        private readonly IStepTemplatesRepository _stepTemplatesRepository;

        public CreateStepCommandHandler(IStepsRepository stepsRepository, IStepTemplatesRepository steptemplatesRepository)
        {
            _stepsRepository = stepsRepository;
            _stepTemplatesRepository = steptemplatesRepository;
        }

        public async Task<CommandResult> Handle(CreateStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var resolvedTemplate = await _stepTemplatesRepository.GetStepTemplateAsync(request.StepTemplateId);

            if (resolvedTemplate == null)
            {
                throw new StepTemplateNotFoundException("Step template " + request.StepTemplateId + " not found.");
            }

            var step = await _stepsRepository.InsertStepAsync(
                resolvedTemplate.GenerateStep(request.StepTemplateId, request.Name, request.Description, request.Inputs, request.Tests)
                );


            await _stepsRepository.InsertJournalEntryAsync(new JournalEntry()
            {
                SubjectId = step.Id,
                ChainId = 0,
                Entity = JournalEntityTypes.Step,
                RecordedOn = DateTime.UtcNow,
                Updates = new List<Update>()
                {
                    new Update()
                    {
                        FieldName = "status",
                        Value = StepStatuses.Unassigned,
                        Type = UpdateType.Override
                    }
                }
            });

            stopwatch.Stop();
            return new CommandResult()
            {
                ObjectRefId = step.Id.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Create
            };
        }
    }
}
