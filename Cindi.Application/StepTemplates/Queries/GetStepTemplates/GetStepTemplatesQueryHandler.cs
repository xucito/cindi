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

namespace Cindi.Application.StepTemplates.Queries.GetStepTemplates
{
    public class GetStepTemplatesQueryHandler : IRequestHandler<GetStepTemplatesQuery, List<StepTemplate>>
    {
        private readonly IStepTemplatesRepository _stepTemplateRepository;

        public GetStepTemplatesQueryHandler(IConfiguration configuration, IStepTemplatesRepository stepTemplateRespository)
        {
            _stepTemplateRepository = stepTemplateRespository;
        }

        public async Task<List<StepTemplate>> Handle(GetStepTemplatesQuery request, CancellationToken cancellationToken)
        {
            return await _stepTemplateRepository.GetStepTemplatesAsync(request.Page, request.Size);
        }
    }
}
