using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.ClusterRPC;
using MediatR;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Cluster.Commands.UpdateClusterState
{
    public class UpdateClusterStateCommandHandler : IRequestHandler<UpdateClusterStateCommand, CommandResult>
    {
        private readonly IStateMachine _stateMachine;
        public UpdateClusterStateCommandHandler(
            IStateMachine stateMachine
            )
        {
            _stateMachine = stateMachine;
        }

        public async Task<CommandResult> Handle(UpdateClusterStateCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var defaultSettings = new ClusterSettings();
            if (request.ResetToDefault)
            {
                _stateMachine.UpdateClusterSettings(new UpdateClusterDetails()
                {
                    AssignmentEnabled = defaultSettings.AssignmentEnabled,
                    AllowAutoRegistration = defaultSettings.AllowAutoRegistration,
                    MetricRetentionPeriod = defaultSettings.MetricRetentionPeriod,
                    StepRetentionPeriod = defaultSettings.StepRetentionPeriod
                });
            }
            else
            {
                _stateMachine.UpdateClusterSettings(new UpdateClusterDetails()
                {
                    AssignmentEnabled = request.AssignmentEnabled,
                    AllowAutoRegistration = request.AllowAutoRegistration,
                    MetricRetentionPeriod = request.MetricRetentionPeriod,
                    StepRetentionPeriod = request.StepRetentionPeriod
                });
            }

            return new CommandResult()
            {
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Update
            };
        }
    }
}
