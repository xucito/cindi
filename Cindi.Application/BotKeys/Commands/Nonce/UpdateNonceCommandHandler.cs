using Cindi.Application.Entities.Queries.GetEntity;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.States;
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
        IMediator _mediator;
        private readonly IEntitiesRepository _entitiesRepository;
        private readonly IStateMachine _stateMachine;

        public UpdateNonceCommandHandler(
            IEntitiesRepository entitiesRepository,
            IStateMachine stateMachine,
            IMediator mediator)
        {
            _entitiesRepository = entitiesRepository;
            _stateMachine = stateMachine;
            _mediator = mediator;
        }

        public async Task<CommandResult> Handle(UpdateNonceCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var key = (await _mediator.Send(new GetEntityQuery<BotKey>()
            {
                Expression = bk => bk.Id == request.Id
            })).Result;

            if (key.Nonce >= request.Nonce)
            {
                throw new InvalidNonceException("Specified nonce " + request.Nonce + " for bot Key " + request.Id + " is less or equal to stored nonce " + request.Nonce);
            }

            key.Nonce = request.Nonce;

            await _entitiesRepository.Update(key);

            return new CommandResult()
            {
                ObjectRefId = key.Id.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Update
            };
        }
    }
}
