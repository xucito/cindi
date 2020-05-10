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

            var entity = await _entitiesRepository.GetFirstOrDefaultAsync(request.Expression);

            if (entity != null)
            {
                var objectLock = await _node.Handle(new RequestDataShard()
                {
                    Type = typeof(T).Name,
                    ObjectId = entity.Id,
                    CreateLock = true,
                    LockTimeoutMs = 3000
                });

                if (objectLock.IsSuccessful)
                {
                    await _node.Handle(new AddShardWriteOperation()
                    {
                        Data = entity,
                        WaitForSafeWrite = true,
                        Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Delete,
                        RemoveLock = true
                    });

                    return new CommandResult()
                    {
                        ObjectRefId = entity.Id.ToString(),
                        Type = CommandResultTypes.Delete,
                        ElapsedMs = stopwatch.ElapsedMilliseconds,
                        IsSuccessful = true
                    };
                }
                else
                {
                    throw new FailedClusterOperationException("Failed to apply cluster operation with for entity " + entity.Id + " of type " + nameof(T));
                }
            }

            return new CommandResult()
            {
                Type = CommandResultTypes.Delete,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                IsSuccessful = false
            };

        }
    }
}
