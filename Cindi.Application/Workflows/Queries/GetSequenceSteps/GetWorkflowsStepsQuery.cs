using Cindi.Application.Results;
using Cindi.Domain.Entities.Steps;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Workflows.Queries.GetWorkflowSteps
{
    public class GetWorkflowStepsQuery: IRequest<QueryResult<List<Step>>>
    {
        public Guid WorkflowId { get; set; }
    }
}
