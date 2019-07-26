using Cindi.Application.Results;
using Cindi.Domain.Entities.Workflows;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Workflows.Queries.GetWorkflows
{
    public class GetWorkflowsQuery: IRequest<QueryResult<List<Workflow>>>
    {
        public int Page { get; set; }
        public int Size { get; set; }
        public string Status { get; set; }
    }
}
