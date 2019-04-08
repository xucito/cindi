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

namespace Cindi.Application.BotKeys.Queries.GetBotKeys
{
    public class GetBotKeysQueryHandler : IRequestHandler<GetBotKeysQuery, QueryResult<List<BotKey>>>
    {
        IBotKeysRepository _botKeyRepository;

        public GetBotKeysQueryHandler(IBotKeysRepository botKeyRepository)
        {
            _botKeyRepository = botKeyRepository;
        }

        public async Task<QueryResult<List<BotKey>>> Handle(GetBotKeysQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var keys = await _botKeyRepository.GetBotKeysAsync(request.Size, request.Page);

            return new QueryResult<List<BotKey>>()
            {
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Result = keys,
                Count = keys.Count
            };
        }
    }
}
