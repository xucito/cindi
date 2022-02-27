using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.Steps;
using Nest;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Cluster.Queries.GetClusterStats
{
    public class GetClusterStatsQueryHandler : IRequestHandler<GetClusterStatsQuery, QueryResult<ClusterStats>>
    {
        ElasticClient _context;

        public GetClusterStatsQueryHandler(ElasticClient context)
        {
            _context = context;
    }
        public async Task<QueryResult<ClusterStats>> Handle(GetClusterStatsQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var stats = new ClusterStats()
            {
                Steps = new StepStats()
                {
                    Suspended = (await _context.CountAsync<Step>(s => s.Query(q => 
                    q.Term(t => t.Status, StepStatuses.Suspended)
                    ))).Count,
                    Unassigned = (await _context.CountAsync<Step>(s => s.Query(q =>
                    q.Term(t => t.Status, StepStatuses.Unassigned)
                    ))).Count,
                    Assigned = (await _context.CountAsync<Step>(s => s.Query(q =>
                    q.Term(t => t.Status, StepStatuses.Assigned)
                    ))).Count,
                    Successful = (await _context.CountAsync<Step>(s => s.Query(q =>
                    q.Term(t => t.Status, StepStatuses.Successful)
                    ))).Count,
                    Warning = (await _context.CountAsync<Step>(s => s.Query(q =>
                    q.Term(t => t.Status, StepStatuses.Warning)
                    ))).Count,
                    Error = (await _context.CountAsync<Step>(s => s.Query(q =>
                    q.Term(t => t.Status, StepStatuses.Error)
                    ))).Count
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
