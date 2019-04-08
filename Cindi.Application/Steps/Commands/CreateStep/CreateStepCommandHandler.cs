using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
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
        private readonly IStepsRepository _stepsRepository;
        private readonly IStepTemplatesRepository _stepTemplatesRepository;
        private readonly IClusterStateService _clusterStateService;

        public CreateStepCommandHandler(IStepsRepository stepsRepository, IStepTemplatesRepository steptemplatesRepository, IClusterStateService service)
        {
            _stepsRepository = stepsRepository;
            _stepTemplatesRepository = steptemplatesRepository;
            _clusterStateService = service;
        }

        public async Task<CommandResult<Step>> Handle(CreateStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var resolvedTemplate = await _stepTemplatesRepository.GetStepTemplateAsync(request.StepTemplateId);

            if (resolvedTemplate == null)
            {
                throw new StepTemplateNotFoundException("Step template " + request.StepTemplateId + " not found.");
            }

            var newStep = resolvedTemplate.GenerateStep(request.StepTemplateId, request.CreatedBy, request.Name, request.Description, request.Inputs, request.Tests);

            newStep.EncryptStepSecrets(EncryptionProtocol.AES256, resolvedTemplate, ClusterStateService.GetEncryptionKey());

            var step = await _stepsRepository.InsertStepAsync(
                newStep
                );

            var update = new JournalEntry()
            {
                SubjectId = step.Id,
                ChainId = 0,
                Entity = JournalEntityTypes.Step,
                CreatedBy = request.CreatedBy,
                CreatedOn = DateTime.UtcNow,
                Updates = new List<Update>()
                {
                    new Update()
                    {
                        FieldName = "status",
                        Value = StepStatuses.Unassigned,
                        Type = UpdateType.Override
                    }
                }
            };


            await _stepsRepository.InsertJournalEntryAsync(update);

            step.Journal.Entries.Add(update);

            await _stepsRepository.UpsertStepMetadataAsync(step.Id);

            stopwatch.Stop();
            return new CommandResult<Step>()
            {
                ObjectRefId = step.Id.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Create,
                Result = step
            };
        }
    }
}
