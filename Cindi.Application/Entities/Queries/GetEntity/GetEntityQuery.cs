using Cindi.Application.Results;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Cindi.Application.Entities.Queries.GetEntity
{
    public class GetEntityQuery<T> : IRequest<QueryResult<T>>
    {
        public Guid Id { get; set; }
        [JsonIgnore]
        public Expression<Func<T, object>> Exclude = null;
    }
}
