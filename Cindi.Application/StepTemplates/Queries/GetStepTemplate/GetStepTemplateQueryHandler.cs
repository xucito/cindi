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

namespace Cindi.Application.StepTemplates.Queries.GetStepTemplate
{
    public class GetStepTemplateQueryHandler : IRequestHandler<GetStepTemplateQuery, StepTemplate>
    {
        private readonly IStepTemplatesRepository _stepTemplateRepository;

        public GetStepTemplateQueryHandler(IConfiguration configuration, IStepTemplatesRepository stepTemplateRepository)
        {
            _stepTemplateRepository = stepTemplateRepository;
        }

        public async Task<StepTemplate> Handle(GetStepTemplateQuery request, CancellationToken cancellationToken)
        {
            var result = await _stepTemplateRepository.GetStepTemplateAsync(request.Name, request.Version);
            return result;
        }
    }
}
