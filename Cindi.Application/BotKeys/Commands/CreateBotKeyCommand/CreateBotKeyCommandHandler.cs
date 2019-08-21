using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Utilities;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.RPCs;
using ConsensusCore.Node;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.BotKeys.Commands.CreateBotKeyCommand
{
    public class CreateBotKeyCommandHandler : IRequestHandler<CreateBotKeyCommand, CommandResult<string>>
    {
        IBotKeysRepository _botKeyRepository;
        IConsensusCoreNode<CindiClusterState, IBaseRepository> _node;
        public CreateBotKeyCommandHandler(IBotKeysRepository botKeyRepository, IConsensusCoreNode<CindiClusterState, IBaseRepository> node)
        {
            _botKeyRepository = botKeyRepository;
            _node = node;
        }

        public async Task<CommandResult<string>> Handle(CreateBotKeyCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            byte[] salt = new byte[128 / 8];

            var plainTextKey = SecurityUtility.RandomString(32, false);
            Guid keyId = Guid.NewGuid();
            var key = (await _node.Send(new WriteData()
            {
                WaitForSafeWrite = true,
                Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Create,
                Data = new Domain.Entities.BotKeys.BotKey()
                {
                    HashedIdKey = SecurityUtility.OneWayHash(plainTextKey, salt),
                    HashedIdKeySalt = salt,
                    PublicEncryptionKey = request.PublicEncryptionKey,
                    BotName = request.BotKeyName,
                    Id = keyId,
                    IsDisabled = false,
                    Nonce = 0,
                    ShardType = typeof(BotKey).Name
        }
            }));

            return new CommandResult<string>()
            {
                ObjectRefId = keyId.ToString(),
                Type = CommandResultTypes.Create,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Result = plainTextKey
            };
        }
    }
}
