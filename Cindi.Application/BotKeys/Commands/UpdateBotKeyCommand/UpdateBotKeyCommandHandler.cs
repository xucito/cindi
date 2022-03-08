using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Exceptions.State;
using Nest;
using MediatR;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cindi.Application.Utilities;

namespace Cindi.Application.BotKeys.Commands.UpdateBotKeyCommand
{
    public class UpdateBotKeyCommandHandler : IRequestHandler<UpdateBotKeyCommand, CommandResult<BotKey>>
    {
        ElasticClient _context;
        public UpdateBotKeyCommandHandler(ElasticClient context)
        {
            _context = context;
        }

        public async Task<CommandResult<BotKey>> Handle(UpdateBotKeyCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var botKey = await _context.LockAndGetObject<BotKey>(request.Id);

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


            if (botKey != null)
            {
                if (update)
                {
                    await _context.IndexDocumentAsync(botKey);
                    

                    return new CommandResult<BotKey>()
                    {
                        ElapsedMs = stopwatch.ElapsedMilliseconds,
                        ObjectRefId = request.Id.ToString(),
                        Type = CommandResultTypes.Update,
                        Result = botKey
                    };
                }
            }
            else
            {
                throw new FailedClusterOperationException("Failed to apply cluster operation with for botkey " + botKey.Id);
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
