using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Exceptions.State;




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
        IEntitiesRepository _entitesRepository;
        public DeleteBotKeyCommandHandler(IEntitiesRepository entitesRepository)
        {
            _entitesRepository = entitesRepository;
        }

        public async Task<CommandResult> Handle(DeleteBotKeyCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await _entitesRepository.DeleteById<BotKey>(request.Id);

            return new CommandResult()
            {
                ObjectRefId = request.Id.ToString(),
                Type = CommandResultTypes.Delete,
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
