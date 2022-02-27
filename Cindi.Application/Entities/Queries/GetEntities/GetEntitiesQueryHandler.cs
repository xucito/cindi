using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Nest;
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
    public class GetEntitiesQueryHandler<T> : IRequestHandler<GetEntitiesQuery<T>, QueryResult<List<T>>> where T : class
    {
        private ElasticClient _context; 

        public GetEntitiesQueryHandler(ElasticClient context)
        {
            _context = context;
        }

        public async Task<QueryResult<List<T>>> Handle(GetEntitiesQuery<T> request, CancellationToken cancellationToken) 
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = (await _context.SearchAsync<T>(request.Expression));
            var entities = result.Hits.Select(h => h.Source).ToList();//, request.Exclusions, request.Sort, request.Size, request.Page)).ToList();
            stopwatch.Stop();

            var countRequest = new CountRequest();
            return new QueryResult<List<T>>()
            {
                Result = entities,
                Count = result.Total,
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
