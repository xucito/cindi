﻿using Cindi.Application.Interfaces;
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
        private IClusterStateService _state;

        public UpdateClusterStateCommandHandler(IClusterStateService state)
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

            if(request.AutoRegistrationEnabled != null)
            {
                _state.SetAllowAutoRegistration(request.AutoRegistrationEnabled.Value);
            }

            return new CommandResult()
            {
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Update
            };
        }
    }
}