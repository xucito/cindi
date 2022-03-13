using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Nest;
using MediatR;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cindi.Domain.Entities.States;
using System.Linq;

namespace Cindi.Application.Cluster.Commands.UpdateClusterState
{
    public class UpdateClusterStateCommandHandler : IRequestHandler<UpdateClusterStateCommand, CommandResult>
    {
        private IClusterStateService _state;
        private ElasticClient _context;

        public UpdateClusterStateCommandHandler(ElasticClient context, IClusterStateService state)
        {
            _state = state;
            _context = context;
        }

        public async Task<CommandResult> Handle(UpdateClusterStateCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var state = (await _context.SearchAsync<CindiClusterState>()).HitsMetadata.Hits.Select(h => h.Source).FirstOrDefault();
            if (request.AssignmentEnabled.HasValue)
                state.Settings.AssignmentEnabled = request.AssignmentEnabled.Value;
            if (request.AllowAutoRegistration.HasValue)
                state.Settings.AllowAutoRegistration = request.AllowAutoRegistration.Value;
            if (!string.IsNullOrEmpty(request.MetricRetentionPeriod))
                state.Settings.MetricRetentionPeriod = request.MetricRetentionPeriod;
            if (!string.IsNullOrEmpty(request.StepRetentionPeriod))
                state.Settings.StepRetentionPeriod = request.StepRetentionPeriod;
            if (request.CleanupInterval != null)
                state.Settings.CleanupInterval = request.CleanupInterval.Value;

            await _context.IndexDocumentAsync<CindiClusterState>(state);

            return new CommandResult()
            {
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Update
            };
        }
    }
}
