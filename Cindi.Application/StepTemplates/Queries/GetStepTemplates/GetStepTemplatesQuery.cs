using Cindi.Application.Results;
using Cindi.Domain.Entities.StepTemplates;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.StepTemplates.Queries.GetStepTemplates
{
    public class GetStepTemplatesQuery: IRequest<QueryResult<List<StepTemplate>>>
    {
        public int Page { get; set; }
        public int Size { get; set; }
    }
}
