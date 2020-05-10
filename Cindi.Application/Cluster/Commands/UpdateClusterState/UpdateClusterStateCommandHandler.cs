using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.ClusterRPC;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.RPCs.Raft;
using ConsensusCore.Node.Communication.Controllers;
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
        private IClusterRequestHandler _node;

        public UpdateClusterStateCommandHandler(IClusterRequestHandler node, IClusterStateService state)
        {
            _state = state;
            _node = node;
        }

        public async Task<CommandResult> Handle(UpdateClusterStateCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await _node.Handle(new ExecuteCommands()
            {
                WaitForCommits = true,
                Commands = new List<BaseCommand>()
                {
                    new UpdateClusterDetails()
                    {
                        AssignmentEnabled =  request.AssignmentEnabled,
                        AllowAutoRegistration = request.AllowAutoRegistration,
                        MetricRetentionPeriod = request.MetricRetentionPeriod,
                        DefaultIfNull = request.DefaultIfNull
                    }
                }
            });

            return new CommandResult()
            {
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Update
            };
        }
    }
}
