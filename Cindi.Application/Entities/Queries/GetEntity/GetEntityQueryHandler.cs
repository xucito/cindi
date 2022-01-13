using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities;
using Cindi.Persistence.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Entities.Queries.GetEntity
{
    public class GetEntityQueryHandler<T> : IRequestHandler<GetEntityQuery<T>, QueryResult<T>> where T: TrackedEntity
    {
        private readonly ApplicationDbContext _context;

        public GetEntityQueryHandler(ApplicationDbContext context)
        {
            _context = context;

        }
        public async Task<QueryResult<T>> Handle(GetEntityQuery<T> request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            DbSet<T> set = _context.Set<T>();
            var result = await set.FirstOrDefaultAsync<T>(request.Expression);

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
