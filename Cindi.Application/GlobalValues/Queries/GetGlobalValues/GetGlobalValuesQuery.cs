using Cindi.Application.Results;
using Cindi.Domain.Entities.GlobalValues;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.GlobalValues.Queries.GetGlobalValues
{
    public class GetGlobalValuesQuery: IRequest<QueryResult<IEnumerable<GlobalValue>>>
    {
        public int Page { get; set; }
        public int Size { get; set; }
    }
}
