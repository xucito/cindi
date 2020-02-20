using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Entities.Queries.GetEntities
{
    public class GetEntitiesQueryHandler<T> : IRequestHandler<GetEntitiesQuery<T>, QueryResult<List<T>>>
    {
        private readonly IEntityRepository _entityRepository;

        public GetEntitiesQueryHandler(IEntityRepository entityRepository)
        {
            _entityRepository = entityRepository;
        }

        public async Task<QueryResult<List<T>>> Handle(GetEntitiesQuery<T> request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            System.Linq.Expressions.Expression<Func<T, bool>> expression;
            if (request.Expression != null)
                expression = request.Expression;
            else
                expression = (s) => true;
            var entities = (await _entityRepository.GetAsync<T>(expression, request.Exclusions, request.Sort, request.Size, request.Page)).ToList();
            stopwatch.Stop();

            return new QueryResult<List<T>>()
            {
                Result = entities,
                Count = _entityRepository.Count<T>(expression),
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
