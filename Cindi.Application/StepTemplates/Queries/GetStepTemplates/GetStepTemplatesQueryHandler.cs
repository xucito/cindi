using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.StepTemplates;
using MediatR;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.StepTemplates.Queries.GetStepTemplates
{
    public class GetStepTemplatesQueryHandler : IRequestHandler<GetStepTemplatesQuery, QueryResult<List<StepTemplate>>>
    {
        private readonly IStepTemplatesRepository _stepTemplateRepository;

        public GetStepTemplatesQueryHandler(IConfiguration configuration, IStepTemplatesRepository stepTemplateRespository)
        {
            _stepTemplateRepository = stepTemplateRespository;
        }

        public async Task<QueryResult<List<StepTemplate>>> Handle(GetStepTemplatesQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var templates = await _stepTemplateRepository.GetStepTemplatesAsync(request.Page, request.Size);

            stopwatch.Stop();
            return new QueryResult<List<StepTemplate>>()
            {
                Result = templates,
                Count = _stepTemplateRepository.CountStepTemplates(),
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
