using Cindi.Application.Entities.Queries.GetEntity;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Exceptions.BotKeys;
using ConsensusCore.Domain.Enums;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.RPCs;
using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Node;
using ConsensusCore.Node.Communication.Controllers;
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
        IClusterRequestHandler _node;

        public UpdateNonceCommandHandler(IMediator mediator, IClusterRequestHandler node)
        {
            _mediator = mediator;
            _node = node;
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

            await _node.Handle(new AddShardWriteOperation()
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
