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

namespace Cindi.Application.BotKeys.Commands.UpdateBotKeyCommand
{
    public class UpdateBotKeyCommandHandler : IRequestHandler<UpdateBotKeyCommand, CommandResult<BotKey>>
    {
        private readonly IStateMachine _stateMachine;
        private readonly IEntitiesRepository _entitiesRepository;
        public UpdateBotKeyCommandHandler(
            IStateMachine stateMachine,
            IEntitiesRepository entitiesRepository)
        {
            _entitiesRepository = entitiesRepository;
            _stateMachine = stateMachine;
        }


        public async Task<CommandResult<BotKey>> Handle(UpdateBotKeyCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var botKey = await _entitiesRepository.GetFirstOrDefaultAsync<BotKey>(bk => bk.Id == request.Id);

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
                await _entitiesRepository.Update(botKey);
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
