﻿using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Utilities;
using Cindi.Persistence;
using Cindi.Persistence.Data;
using MediatR;
using Nest;
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
        ElasticClient _context;
        public CreateBotKeyCommandHandler(ElasticClient context)
        {
            _context = context;
        }

        public async Task<CommandResult<string>> Handle(CreateBotKeyCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            byte[] salt = new byte[128 / 8];
            Random rand = new Random();

            var plainTextKey = SecurityUtility.RandomString(32, false);
            Guid keyId = Guid.NewGuid();
            var key = await _context.IndexAsync(new Domain.Entities.BotKeys.BotKey()
                {
                    HashedIdKey = SecurityUtility.OneWayHash(plainTextKey, salt),
                    HashedIdKeySalt = salt,
                    PublicEncryptionKey = request.PublicEncryptionKey,
                    BotName = (request.BotKeyName == null || request.BotKeyName == "") ? BotGeneratorUtility.GenerateName(rand.Next(4,12)) : request.BotKeyName,
                    Id = keyId,
                    IsDisabled = false,
                    Nonce = 0,
                    RegisteredOn = DateTime.Now
            });

            await _context.SaveChangesAsync();

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
