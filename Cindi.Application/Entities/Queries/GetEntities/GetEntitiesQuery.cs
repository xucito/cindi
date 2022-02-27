using Cindi.Application.Results;
using MediatR;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Cindi.Application.Entities.Queries
{
    public class GetEntitiesQuery<T> : MediatR.IRequest<QueryResult<List<T>>> where T : class
    {
        public Func<SearchDescriptor<T>, ISearchRequest> Expression { get; set; } = null;
    }
}
