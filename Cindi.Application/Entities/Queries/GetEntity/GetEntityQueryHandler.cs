using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities;
using Nest;
using MediatR;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Cindi.Application.Entities.Queries.GetEntity
{
    public class GetEntityQueryHandler<T> : IRequestHandler<GetEntityQuery<T>, QueryResult<T>> where T: TrackedEntity
    {
        private readonly ElasticClient _context;

        public GetEntityQueryHandler(ElasticClient context)
        {
            _context = context;

        }
        public async Task<QueryResult<T>> Handle(GetEntityQuery<T> request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = (await _context.SearchAsync<T>(request.Expression)).Hits.Select(h => h.Source).FirstOrDefault();

            stopwatch.Stop();
            return new QueryResult<T>()
            {
                Result = result,
                Count = result == null ? 0 : 1,
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
