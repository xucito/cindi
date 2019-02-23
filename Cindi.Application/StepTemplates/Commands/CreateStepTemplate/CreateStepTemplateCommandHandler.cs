using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.StepTemplates;
using MediatR;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.StepTemplates.Commands.CreateStepTemplate
{
    public class CreateStepTemplateCommandHandler : IRequestHandler<CreateStepTemplateCommand, StepTemplate>
    {
        private readonly IStepTemplatesRepository _stepTemplateRepository;

        public CreateStepTemplateCommandHandler(IConfiguration configuration, IStepTemplatesRepository client)
        {
            _stepTemplateRepository = client;
        }

        public async Task<StepTemplate> Handle(CreateStepTemplateCommand request, CancellationToken cancellationToken)
        {
            var result = await _stepTemplateRepository.InsertAsync(new StepTemplate()
            {
                Name = request.Name,
                Version = request.Version,
                AllowDynamicInputs = request.AllowDynamicInputs,
                InputDefinitions = request.InputDefinitions,
                OutputDefinitions = request.OutputDefinitions
            });

            return result;
        }
    }
}
