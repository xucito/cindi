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

namespace Cindi.Application.BotKeys.Commands.DeleteBotKeyCommand
{
    public class DeleteBotKeyCommandHandler : IRequestHandler<DeleteBotKeyCommand, CommandResult>
    {
        ElasticClient _context;
        public DeleteBotKeyCommandHandler(ElasticClient context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(DeleteBotKeyCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var lockid = await _context.LockObject<BotKey>(request.Id);

            if (lockid != null)
            {
                await _context.DeleteAsync<BotKey>(request.Id);
                

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
