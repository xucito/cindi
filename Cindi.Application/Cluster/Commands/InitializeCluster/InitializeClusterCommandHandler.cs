using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Application.Users.Commands.CreateUserCommand;
using Cindi.Domain.Entities.States;


using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Cluster.Commands.InitializeCluster
{
    public class InitializeClusterCommandHandler : IRequestHandler<InitializeClusterCommand, CommandResult<NewClusterResult>>
    {
        ILogger<InitializeClusterCommandHandler> _logger;
        private IMediator _mediator;
        private IClusterStateService _clusterState;

        public InitializeClusterCommandHandler(
        ILogger<InitializeClusterCommandHandler> logger,
        IMediator mediator,
        IClusterStateService clusterState
        )
        {
            _mediator = mediator;
            _logger = logger;
            _clusterState = clusterState;
        }

        public async Task<CommandResult<NewClusterResult>> Handle(InitializeClusterCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await _mediator.Send(new CreateUserCommand()
            {
                Username = "admin",
                Password = request.DefaultPassword
            });

            string key = "";

            //_clusterState.SetClusterName(request.Name);

            ClusterStateService.Initialized = true;

            return new CommandResult<NewClusterResult>()
            {
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Create,
                ObjectRefId = request.Name,
                Result = new NewClusterResult()
                {
                    Name = request.Name,
                    EncryptionKey = key
                }
            };
        }
    }
}
