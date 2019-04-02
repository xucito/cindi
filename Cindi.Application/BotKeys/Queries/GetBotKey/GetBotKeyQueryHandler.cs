using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.BotKeys;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.BotKeys.Queries.GetBotKey
{
    public class GetBotKeyQueryHandler: IRequestHandler<GetBotKeyQuery, QueryResult<BotKey>>
    {
        IBotKeysRepository _botKeyRepository;

        public GetBotKeyQueryHandler(IBotKeysRepository botKeyRepository)
        {
            _botKeyRepository = botKeyRepository;
        }

        public async Task<QueryResult<BotKey>> Handle(GetBotKeyQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var key = await _botKeyRepository.GetBotKeyAsync(request.Id);

            return new QueryResult<BotKey>()
            {
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Result = key,
                Count = key != null ? 1 : 0
            };
        }
    }
}
