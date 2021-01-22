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
        IEntitiesRepository _entitiesRepository;
        ILogger<InitializeClusterCommandHandler> _logger;
        private readonly IStateMachine _stateMachine;
        private IMediator _mediator;

        public InitializeClusterCommandHandler(
        ILogger<InitializeClusterCommandHandler> logger,
            IStateMachine stateMachine,
        IEntitiesRepository entitiesRepository,
        IMediator mediator
        )
        {
            _mediator = mediator;
            _entitiesRepository = entitiesRepository;
            _logger = logger;
            _stateMachine = stateMachine;
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

            _stateMachine.SetInitialized(true);

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
