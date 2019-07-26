using Cindi.Application.Results;
using Cindi.Domain.Entities.WorkflowsTemplates;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.WorkflowTemplates.Queries.GetWorkflowTemplates
{
    public class GetWorkflowTemplatesQuery: IRequest<QueryResult<List<WorkflowTemplate>>>
    {
        public int Page { get; set; }
        public int Size { get; set; }
    }
}
