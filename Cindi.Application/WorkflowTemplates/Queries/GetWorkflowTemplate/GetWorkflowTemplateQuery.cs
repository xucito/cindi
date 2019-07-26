using Cindi.Application.Results;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Entities.WorkflowsTemplates;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.WorkflowTemplates.Queries.GetWorkFlowTemplate
{
    public class GetWorkflowTemplateQuery: IRequest<QueryResult<WorkflowTemplate>>
    {
        public string Id { get; set; }
    }
}
