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

namespace Cindi.Application.StepTemplates.Queries.GetStepTemplate
{
    public class GetStepTemplateQueryHandler : IRequestHandler<GetStepTemplateQuery, QueryResult<StepTemplate>>
    {
        private readonly IStepTemplatesRepository _stepTemplateRepository;

        public GetStepTemplateQueryHandler(IConfiguration configuration, IStepTemplatesRepository stepTemplateRepository)
        {
            _stepTemplateRepository = stepTemplateRepository;
        }

        public async Task<QueryResult<StepTemplate>> Handle(GetStepTemplateQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = await _stepTemplateRepository.GetStepTemplateAsync(request.Id);
            stopwatch.Stop();

            return new QueryResult<StepTemplate>()
            {
                Result = result,
                Count = result == null ? 0 : 1,
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
