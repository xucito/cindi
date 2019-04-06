using Cindi.Application.BotKeys.Queries.GetBotKey;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Exceptions.BotKeys;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.BotKeys.Commands.Nonce
{
    public class UpdateNonceCommandHandler : IRequestHandler<UpdateNonceCommand, CommandResult>
    {
        IBotKeysRepository _botKeyRepository;
        IMediator _mediator;

        public UpdateNonceCommandHandler(IBotKeysRepository botKeyRepository, IMediator mediator)
        {
            _botKeyRepository = botKeyRepository;
            _mediator = mediator;
        }

        public async Task<CommandResult> Handle(UpdateNonceCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var key = (await _mediator.Send(new GetBotKeyQuery()
            {
                Id = request.Id
            })).Result;

            if (key.Nonce >= request.Nonce)
            {
                throw new InvalidNonceException("Specified nonce " + request.Nonce + " for bot Key " + request.Id + " is less or equal to stored nonce " + request.Nonce);
            }

            key.Nonce = request.Nonce;

            await _botKeyRepository.UpdateBotKey(key);

            return new CommandResult()
            {
                ObjectRefId = key.Id.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Update
            };
        }
    }
}
