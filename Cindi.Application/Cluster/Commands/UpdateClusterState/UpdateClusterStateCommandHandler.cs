using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Persistence.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Cluster.Commands.UpdateClusterState
{
    public class UpdateClusterStateCommandHandler : IRequestHandler<UpdateClusterStateCommand, CommandResult>
    {
        private IClusterStateService _state;
        private ApplicationDbContext _context;

        public UpdateClusterStateCommandHandler(ApplicationDbContext context, IClusterStateService state)
        {
            _state = state;
            _context = context;
        }

        public async Task<CommandResult> Handle(UpdateClusterStateCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var state = await _context.CindiClusterStates.FirstOrDefaultAsync();
            if (request.AssignmentEnabled.HasValue)
                state.Settings.AssignmentEnabled = request.AssignmentEnabled.Value;
            if (request.AllowAutoRegistration.HasValue)
                state.Settings.AllowAutoRegistration = request.AllowAutoRegistration.Value;
            if (!string.IsNullOrEmpty(request.MetricRetentionPeriod))
                state.Settings.MetricRetentionPeriod = request.MetricRetentionPeriod;

            return new CommandResult()
            {
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Update
            };
        }
    }
}
