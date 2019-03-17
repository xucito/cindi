using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using MediatR;
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
        private ClusterStateService _state;

        public UpdateClusterStateCommandHandler(ClusterStateService state)
        {
            _state = state;
        }

        public async Task<CommandResult> Handle(UpdateClusterStateCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (request.AssignmentEnabled != null)
            {
                _state.ChangeAssignmentEnabled(request.AssignmentEnabled.Value);
            }

            return new CommandResult()
            {
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Update
            };
        }
    }
}
