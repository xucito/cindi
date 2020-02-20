using Cindi.Application.Results;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Cindi.Application.Entities.Queries
{
    public class GetEntitiesQuery<T> : IRequest<QueryResult<List<T>>>
    {
        public Type Entity { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
        public Expression<Func<T, bool>> Expression { get; set; }
        [JsonIgnore]
        public List<Expression<Func<T, object>>> Exclusions = null;
        public string Sort { get; set; } = null;
    }
}
