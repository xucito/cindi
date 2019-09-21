using Cindi.Application.Results;
using Cindi.Domain.Entities.Steps;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Cindi.Application.Steps.Queries.GetStep
{
    public class GetStepQuery: IRequest<QueryResult<Step>>
    {
        public Guid Id { get; set; }
        [JsonIgnore]
        public Expression<Func<Step, object>> Exclude = null;
    }
}
