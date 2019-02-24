using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Exceptions.StepTemplates;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Steps.Commands
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

            var resolvedTemplate = await _stepTemplatesRepository.GetStepTemplateAsync(request.TemplateReference.Name, request.TemplateReference.Version);

            if (resolvedTemplate == null)
            {
                throw new StepTemplateNotFoundException("Step template " + request.TemplateReference.TemplateId + " not found.");
            }

            var result = await _stepsRepository.InsertStepAsync(
                resolvedTemplate.GenerateStep(request.TemplateReference, request.Name, request.Description, request.Inputs, request.Tests)
                );

            stopwatch.Stop();
            return new CommandResult()
            {
                ObjectRefId = result.Id.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Create
            };
        }
    }
}
