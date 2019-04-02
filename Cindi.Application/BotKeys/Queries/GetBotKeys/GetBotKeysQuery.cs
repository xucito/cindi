using Cindi.Application.Results;
using Cindi.Domain.Entities.BotKeys;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.BotKeys.Queries.GetBotKeys
{
    public class GetBotKeysQuery: IRequest<QueryResult<List<BotKey>>>
    {
        public int Page { get; set; }
        public int Size { get; set; }
    }
}
