using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.Steps;
using Cindi.Persistence.Data;
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
        ApplicationDbContext _context;

        public GetClusterStatsQueryHandler(ApplicationDbContext context)
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
                    Suspended = _context.Steps.Count(s => s.Status == StepStatuses.Suspended),
                    Unassigned = _context.Steps.Count<Step>(s => s.Status == StepStatuses.Unassigned),
                    Assigned = _context.Steps.Count<Step>(s => s.Status ==StepStatuses.Assigned),
                    Successful = _context.Steps.Count<Step>(s => s.Status ==StepStatuses.Successful),
                    Warning = _context.Steps.Count<Step>(s => s.Status ==StepStatuses.Warning),
                    Error = _context.Steps.Count<Step>(s => s.Status ==StepStatuses.Error),
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
