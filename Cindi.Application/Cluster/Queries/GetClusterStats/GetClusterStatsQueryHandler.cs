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
        IStepsRepository _stepsRepository;

        public GetClusterStatsQueryHandler(IStepsRepository stepsRepository)
        {
            _stepsRepository = stepsRepository;
        }
        public async Task<QueryResult<ClusterStats>> Handle(GetClusterStatsQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var stats = new ClusterStats()
            {
                Steps = new StepStats()
                {
                    Suspended = _stepsRepository.CountSteps(StepStatuses.Suspended),
                    Unassigned = _stepsRepository.CountSteps(StepStatuses.Unassigned),
                    Assigned = _stepsRepository.CountSteps(StepStatuses.Assigned),
                    Successful = _stepsRepository.CountSteps(StepStatuses.Successful),
                    Warning = _stepsRepository.CountSteps(StepStatuses.Warning),
                    Error = _stepsRepository.CountSteps(StepStatuses.Error),
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
