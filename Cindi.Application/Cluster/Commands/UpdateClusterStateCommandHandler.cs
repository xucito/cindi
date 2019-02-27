using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Cluster.Commands
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
            if(request.StepTemplateAssignmentUpdates != null)
            {
                _state.UpdateStepAssignmentCheckpoints(request.StepTemplateAssignmentUpdates);
            }

            return new CommandResult() { };
        }
    }
}
