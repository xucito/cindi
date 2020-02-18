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
        private readonly IEntityRepository _entityRepository;

        public GetStepsQueryHandler(IEntityRepository entityRepository)
        {
            _entityRepository = entityRepository;
        }

        public async Task<QueryResult<List<Step>>> Handle(GetStepsQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            System.Linq.Expressions.Expression<Func<Step, bool>> expression;
            if (request.Status != null)
                expression = (s) => request.Status == null;
            else
                expression = (s) => true;
            var steps = (await _entityRepository.GetAsync<Step>(expression, request.Exclusions, request.Sort, request.Size, request.Page)).OrderByDescending(s => s.CreatedOn).ToList();
            stopwatch.Stop();

            return new QueryResult<List<Step>>()
            {
                Result = steps,
                Count = _entityRepository.Count<Step>(expression),
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
