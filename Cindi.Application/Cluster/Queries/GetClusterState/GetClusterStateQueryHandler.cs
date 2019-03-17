using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Cluster.Queries.GetClusterState
{
    public class GetClusterStateQueryHandler : IRequestHandler<GetClusterStateQuery, QueryResult<ClusterState>>
    {
        ClusterStateService _clusterStateService;
        public GetClusterStateQueryHandler(ClusterStateService clusterStateService)
        {
            _clusterStateService = clusterStateService;
        }

        public async Task<QueryResult<ClusterState>> Handle(GetClusterStateQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var state = _clusterStateService.GetState();
            
            return new QueryResult<ClusterState>()
            {
                Result = state,
                Count = 1,
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
