using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using ConsensusCore.Domain.BaseClasses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Entities.Queries.GetEntity
{
    public class GetEntityQueryHandler<T> : IRequestHandler<GetEntityQuery<T>, QueryResult<T>> where T: ShardData
    {
        private readonly IEntityRepository _entityRepository;

        public GetEntityQueryHandler(IEntityRepository entityRepository)
        {
            _entityRepository = entityRepository;

        }
        public async Task<QueryResult<T>> Handle(GetEntityQuery<T> request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = await _entityRepository.GetFirstOrDefaultAsync<T>(e => e.Id == request.Id);

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
