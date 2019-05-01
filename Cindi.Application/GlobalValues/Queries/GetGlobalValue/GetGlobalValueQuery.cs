using Cindi.Application.Results;
using Cindi.Domain.Entities.GlobalValues;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.GlobalValues.Queries.GetGlobalValue
{
    public class GetGlobalValueQuery : IRequest<QueryResult<GlobalValue>>
    {
        public string Name { get; set; }
    }
}
