using Cindi.Application.Results;
using Cindi.Domain.Entities.BotKeys;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.BotKeys.Queries.GetBotKey
{
    public class GetBotKeyQuery: IRequest<QueryResult<BotKey>>
    {
        public Guid Id { get; set; }
    }
}
