using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Exceptions.State;
using ConsensusCore.Domain.RPCs;
using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Node;
using ConsensusCore.Node.Communication.Controllers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.BotKeys.Commands.DeleteBotKeyCommand
{
    public class DeleteBotKeyCommandHandler : IRequestHandler<DeleteBotKeyCommand, CommandResult>
    {
        IBotKeysRepository _botKeyRepository;
        IClusterRequestHandler _node;
        public DeleteBotKeyCommandHandler(IBotKeysRepository botKeyRepository, IClusterRequestHandler node)
        {
            _botKeyRepository = botKeyRepository;
            _node = node;
        }

        public async Task<CommandResult> Handle(DeleteBotKeyCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var botLockResult = await _node.Handle(new RequestDataShard()
            {
                Type = "BotKey",
                ObjectId = request.Id,
                CreateLock = true
            });

            if (botLockResult.IsSuccessful)
            {
                await _node.Handle(new AddShardWriteOperation()
                {
                    Data = botLockResult.Data,
                    WaitForSafeWrite = true,
                    Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Delete,
                    RemoveLock = true
                });

                return new CommandResult()
                {
                    ObjectRefId = request.Id.ToString(),
                    Type = CommandResultTypes.Delete,
                    ElapsedMs = stopwatch.ElapsedMilliseconds
                };
            }
            else
            {
                throw new FailedClusterOperationException("Failed to apply cluster operation with for botkey " + request.Id);
            }
        }
    }
}
