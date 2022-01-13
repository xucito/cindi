using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Persistence.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        private ApplicationDbContext _context; 

        public GetEntitiesQueryHandler(ApplicationDbContext context)
        {
            _context = context;
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
            var entities = await _context.GetEntitySet<T>().Where(expression).Skip(request.Page * request.Size).Take(request.Page).ToListAsync();//, request.Exclusions, request.Sort, request.Size, request.Page)).ToList();
            stopwatch.Stop();

            return new QueryResult<List<T>>()
            {
                Result = entities,
                Count = await _context.GetEntitySet<T>().Where(expression).Skip(request.Page * request.Size).Take(request.Page).CountAsync(),
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
