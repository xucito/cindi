using Cindi.Application.Results;
using Cindi.Domain.Entities.Steps;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Steps.Queries.GetSteps
{
    public class GetStepsQuery: IRequest<QueryResult<List<Step>>>
    {
        public int Page { get; set; }
        public int Size { get; set; }
    }
}
