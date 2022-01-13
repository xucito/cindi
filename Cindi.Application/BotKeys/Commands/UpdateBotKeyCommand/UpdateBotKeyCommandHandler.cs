using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Exceptions.State;
using Cindi.Persistence.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        ApplicationDbContext _context;
        public UpdateBotKeyCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CommandResult<BotKey>> Handle(UpdateBotKeyCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var botKey = await _context.LockObject<BotKey>(request.Id);

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
                    _context.Update(botKey);
                    await _context.SaveChangesAsync();

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
