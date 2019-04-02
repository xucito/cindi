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
        ClusterStateService _clusterStateService;

        public SetEncryptionKeyCommandHandler(ClusterStateService clusterStateService)
        {
            _clusterStateService = clusterStateService;
        }

        public async Task<CommandResult> Handle(SetEncryptionKeyCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();

            _clusterStateService.SetEncryptionKey(request.EncryptionKey);

            return new CommandResult()
            {
                ObjectRefId = _clusterStateService.GetState().Id,
                Type = CommandResultTypes.Update,
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
