using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Exceptions.Utility;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Cluster.Commands.SetEncryptionKey
{
    public class SetEncryptionKeyCommandHandler : IRequestHandler<SetEncryptionKeyCommand, CommandResult>
    {
        IStateMachine _stateMachine;

        public SetEncryptionKeyCommandHandler(IStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public async Task<CommandResult> Handle(SetEncryptionKeyCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();

            _stateMachine.SetEncryptionKey(request.EncryptionKey);

            return new CommandResult()
            {
                ObjectRefId = _stateMachine.GetState().Id.ToString(),
                Type = CommandResultTypes.Update,
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
