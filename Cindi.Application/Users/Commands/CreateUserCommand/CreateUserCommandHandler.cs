using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Diagnostics;
using Cindi.Domain.Utilities;
using Cindi.Domain.Entities.States;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Node;
using ConsensusCore.Domain.RPCs;
using ConsensusCore.Node.Services;
using ConsensusCore.Node.Communication.Controllers;
using ConsensusCore.Domain.RPCs.Shard;

namespace Cindi.Application.Users.Commands.CreateUserCommand
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CommandResult>
    {
        IUsersRepository _usersRepository;
        IClusterRequestHandler _node;

        public CreateUserCommandHandler(
            IUsersRepository usersRepository,
            ILogger<CreateUserCommandHandler> logger,
            IServiceProvider prov,
            IDataRouter router,
            IClusterRequestHandler node
    )
        {
            _usersRepository = usersRepository;
            _node = (IClusterRequestHandler)prov.GetService(typeof(IClusterRequestHandler));
        }

        public async Task<CommandResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var salt = SecurityUtility.GenerateSalt(128);
            Guid id = Guid.NewGuid();
            var createdUser = await _node.Handle(new AddShardWriteOperation()
            {
                WaitForSafeWrite = true,
                Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Create,
                Data = new Domain.Entities.Users.User(
                request.Username.ToLower(),
                SecurityUtility.OneWayHash(request.Password, salt),
                request.Username.ToLower(),
                salt,
                request.CreatedBy,
                DateTime.UtcNow,
                id
            ),

            });

            return new CommandResult()
            {
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                ObjectRefId = id.ToString(),
                Type = CommandResultTypes.Create
            };
        }
    }
}
