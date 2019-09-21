using Cindi.Application.BotKeys.Queries.GetBotKey;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Exceptions.BotKeys;
using ConsensusCore.Domain.Enums;
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

namespace Cindi.Application.BotKeys.Commands.Nonce
{
    public class UpdateNonceCommandHandler : IRequestHandler<UpdateNonceCommand, CommandResult>
    {
        IBotKeysRepository _botKeyRepository;
        IMediator _mediator;
        IConsensusCoreNode<CindiClusterState, IBaseRepository<CindiClusterState>> _node;

        public UpdateNonceCommandHandler(IBotKeysRepository botKeyRepository, IMediator mediator, IConsensusCoreNode<CindiClusterState, IBaseRepository<CindiClusterState>> node)
        {
            _botKeyRepository = botKeyRepository;
            _mediator = mediator;
            _node = node;
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

            await _node.Handle(new WriteData()
            {
                WaitForSafeWrite = true,
                Data = key,
                Operation = ShardOperationOptions.Update,
            });

           // await _botKeyRepository.UpdateBotKey(key);

            return new CommandResult()
            {
                ObjectRefId = key.Id.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Update
            };
        }
    }
}
