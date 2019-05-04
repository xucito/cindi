using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Utilities;
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

        public CreateBotKeyCommandHandler(IBotKeysRepository botKeyRepository )
        {
            _botKeyRepository = botKeyRepository;
        }

        public async Task<CommandResult<string>> Handle(CreateBotKeyCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            byte[] salt = new byte[128 / 8];

            var plainTextKey = SecurityUtility.RandomString(32, false);

            var key = await _botKeyRepository.InsertBotKeyAsync(new Domain.Entities.BotKeys.BotKey()
            {
                HashedIdKey = SecurityUtility.OneWayHash(plainTextKey, salt),
                HashedIdKeySalt = salt,
                PublicEncryptionKey = request.PublicEncryptionKey,
                BotName = request.BotKeyName,
                Id = Guid.NewGuid(),
                IsDisabled = false,
                Nonce = 0
            });

            return new CommandResult<string>()
            {
                ObjectRefId = key.ToString(),
                Type = CommandResultTypes.Create,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Result = plainTextKey
            };
        }
    }
}
