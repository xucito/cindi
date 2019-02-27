using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Cluster.Queries
{
    public class GetStepAssignmentCheckpointsQuery: IRequest<QueryResult<Dictionary<string, DateTime?>>>
    {
        public List<string> StepTemplateIds { get; set; }
    }
}
