using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.Steps;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Steps.Queries.GetSteps
{
    public class GetStepsQueryHandler : IRequestHandler<GetStepsQuery, QueryResult<List<Step>>>
    {
        private readonly IStepsRepository _stepsRepository;

        public GetStepsQueryHandler(IStepsRepository stepsRepository)
        {
            _stepsRepository = stepsRepository;
        }

        public async Task<QueryResult<List<Step>>> Handle(GetStepsQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var steps = (await _stepsRepository.GetStepsAsync(request.Size, request.Page, request.Status, null, request.Exclusions)).OrderByDescending(s => s.CreatedOn).ToList();
            var stepCount = _stepsRepository.CountSteps(request.Status);
            stopwatch.Stop();

            return new QueryResult<List<Step>>()
            {
                Result = steps,
                Count = stepCount,
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
