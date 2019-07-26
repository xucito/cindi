using Cindi.Application.Results;
using Cindi.Domain.Entities.Workflows;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Workflows.Queries.GetWorkflow
{
    public class GetWorkflowQuery: IRequest<QueryResult<Workflow>>
    {
        public Guid Id;
    }
}
