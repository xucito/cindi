using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.Steps;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Cluster.Queries.GetClusterStats
{
    public class GetClusterStatsQueryHandler : IRequestHandler<GetClusterStatsQuery, QueryResult<ClusterStats>>
    {
        IEntityRepository _entityRepository;

        public GetClusterStatsQueryHandler(IEntityRepository entityRepository)
        {
            _entityRepository = entityRepository;
        }
        public async Task<QueryResult<ClusterStats>> Handle(GetClusterStatsQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var stats = new ClusterStats()
            {
                Steps = new StepStats()
                {
                    Suspended = _entityRepository.Count<Step>(s => s.Status == StepStatuses.Suspended),
                    Unassigned = _entityRepository.Count<Step>(s => s.Status == StepStatuses.Unassigned),
                    Assigned = _entityRepository.Count<Step>(s => s.Status ==StepStatuses.Assigned),
                    Successful = _entityRepository.Count<Step>(s => s.Status ==StepStatuses.Successful),
                    Warning = _entityRepository.Count<Step>(s => s.Status ==StepStatuses.Warning),
                    Error = _entityRepository.Count<Step>(s => s.Status ==StepStatuses.Error),
                }

            };

            return new QueryResult<ClusterStats>()
            {
                Result = stats,
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
