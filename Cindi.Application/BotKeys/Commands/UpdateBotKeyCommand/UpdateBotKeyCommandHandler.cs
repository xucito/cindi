using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Exceptions.State;
using ConsensusCore.Domain.RPCs;
using ConsensusCore.Node;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.BotKeys.Commands.UpdateBotKeyCommand
{
    public class UpdateBotKeyCommandHandler : IRequestHandler<UpdateBotKeyCommand, CommandResult<BotKey>>
    {
        IBotKeysRepository _botKeyRepository;
        IConsensusCoreNode<CindiClusterState> _node;

        public UpdateBotKeyCommandHandler(IBotKeysRepository botKeyRepository, IConsensusCoreNode<CindiClusterState> node)
        {
            _botKeyRepository = botKeyRepository;
            _node = node;
        }

        public async Task<CommandResult<BotKey>> Handle(UpdateBotKeyCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var botKey = await _botKeyRepository.GetBotKeyAsync(request.Id);

            var update = false;

            if (request.IsDisabled != null && botKey.IsDisabled != request.IsDisabled)
            {
                botKey.IsDisabled = request.IsDisabled.Value;
                update = true;
            }

            if (request.BotName != null && botKey.BotName != request.BotName)
            {
                botKey.BotName = request.BotName;
                update = true;
            }

            if (update)
            {
                var botLockResult = await _node.Handle(new RequestDataShard()
                {
                    Type = "BotKey",
                    ObjectId = request.Id,
                    CreateLock = true
                });

                if (botLockResult.IsSuccessful && botLockResult.AppliedLocked)
                {
                    if (update)
                    {
                        var result = await _node.Handle(new WriteData()
                        {
                            Data = botKey,
                            WaitForSafeWrite = true,
                            Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Update,
                            RemoveLock = true
                        });

                        if (result.IsSuccessful)
                        {
                            return new CommandResult<BotKey>()
                            {
                                ElapsedMs = stopwatch.ElapsedMilliseconds,
                                ObjectRefId = request.Id.ToString(),
                                Type = CommandResultTypes.Update,
                                Result = botKey
                            };
                        }
                        else
                        {
                            throw new FailedClusterOperationException("Failed to apply cluster operation with for botKey " + request.Id);
                        }
                    }
                }
                else
                {
                    throw new FailedClusterOperationException("Failed to apply cluster operation with for botkey " + botKey.Id);
                }
            }

            return new CommandResult<BotKey>()
            {
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                ObjectRefId = request.Id.ToString(),
                Type = CommandResultTypes.Update,
                Result = botKey
            };
        }
    }
}
