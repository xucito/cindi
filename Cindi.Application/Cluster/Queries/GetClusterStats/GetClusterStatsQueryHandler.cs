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
        IEntitiesRepository _entitiesRepository;

        public GetClusterStatsQueryHandler(IEntitiesRepository entitiesRepository)
        {
            _entitiesRepository = entitiesRepository;
        }
        public async Task<QueryResult<ClusterStats>> Handle(GetClusterStatsQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var stats = new ClusterStats()
            {
                Steps = new StepStats()
                {
                    Suspended = _entitiesRepository.Count<Step>(s => s.Status == StepStatuses.Suspended),
                    Unassigned = _entitiesRepository.Count<Step>(s => s.Status == StepStatuses.Unassigned),
                    Assigned = _entitiesRepository.Count<Step>(s => s.Status ==StepStatuses.Assigned),
                    Successful = _entitiesRepository.Count<Step>(s => s.Status ==StepStatuses.Successful),
                    Warning = _entitiesRepository.Count<Step>(s => s.Status ==StepStatuses.Warning),
                    Error = _entitiesRepository.Count<Step>(s => s.Status ==StepStatuses.Error),
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
