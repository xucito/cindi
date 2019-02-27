using Cindi.Application.Results;
using Cindi.Persistence.SequenceTemplates;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.SequenceTemplates.Commands.CreateSequenceTemplate
{
    public class CreateSequenceTemplateCommandHandler : IRequestHandler<CreateSequenceTemplateCommand, CommandResult>
    {
        private readonly ISequenceTemplatesRepository _sequenceTemplatesRepository;

        public CreateSequenceTemplateCommandHandler(ISequenceTemplatesRepository sequenceTemplatesRepository)
        {
            _sequenceTemplatesRepository = sequenceTemplatesRepository;
        }

        public async Task<CommandResult> Handle(CreateSequenceTemplateCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var createdStep = await _sequenceTemplatesRepository.InsertSequenceTemplateAsync(new Domain.Entities.SequencesTemplates.SequenceTemplate()
            {
                Id = request.Name + ":" + request.Version,
                Description = request.Description,
                InputDefinitions = request.InputDefinitions,
                LogicBlocks = request.LogicBlocks
            });

            stopwatch.Stop();
            return new CommandResult()
            {
                ObjectRefId = createdStep.Id,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Create
            };
        }
    }
}
