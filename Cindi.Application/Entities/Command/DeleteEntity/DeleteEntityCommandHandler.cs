using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterOperation;
using Cindi.Domain.Exceptions.State;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Node.Communication.Controllers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Entities.Command.DeleteEntity
{
    public class DeleteEntityCommandHandler<T> : IRequestHandler<DeleteEntityCommand<T>, CommandResult>
        where T : ShardData
    {
        ClusterService _clusterService;
        public DeleteEntityCommandHandler(ClusterService clusterService)
        {
            _clusterService = clusterService;
        }

        public async Task<CommandResult> Handle(DeleteEntityCommand<T> request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var result = await _clusterService.AddWriteOperation(new EntityWriteOperation<T>()
            {
                Data = (T) request.Entity,
                WaitForSafeWrite = true,
                Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Delete,
                RemoveLock = false
            });

            return new CommandResult()
            {
                ObjectRefId = request.Entity.Id.ToString(),
                Type = CommandResultTypes.Delete,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                IsSuccessful = result.IsSuccessful
            };

        }
    }
}
