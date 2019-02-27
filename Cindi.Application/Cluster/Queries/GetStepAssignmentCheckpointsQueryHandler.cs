using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Cluster.Queries
{
    public class GetStepAssignmentCheckpointsQueryHandler : IRequestHandler<GetStepAssignmentCheckpointsQuery, QueryResult<Dictionary<string, DateTime?>>>
    {
        private ClusterStateService _state;

        public GetStepAssignmentCheckpointsQueryHandler(ClusterStateService state)
        {
            _state = state;
        }

        public async Task<QueryResult<Dictionary<string, DateTime?>>> Handle(GetStepAssignmentCheckpointsQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var results = _state.GetLastStepAssignmentCheckpoints(request.StepTemplateIds.ToArray());
            stopwatch.Stop();

            return new QueryResult<Dictionary<string, DateTime?>>
            {
                Result = results,
                Count = results.Count,
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
