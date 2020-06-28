using Cindi.Application.Interfaces;
using Cindi.Application.Results;
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
        IClusterRequestHandler _node;
        private readonly IEntitiesRepository _entitiesRepository;
        public DeleteEntityCommandHandler(IClusterRequestHandler node, IEntitiesRepository entitiesRepository)
        {
            _node = node;
            _entitiesRepository = entitiesRepository;
        }

        public async Task<CommandResult> Handle(DeleteEntityCommand<T> request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var result = await _node.Handle(new AddShardWriteOperation()
            {
                Data = request.Entity,
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
