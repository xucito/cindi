using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Workflows.Commands.ScanWorkflow
{
    public class ScanWorkflowCommand : IRequest<CommandResult>
    {
        public Guid WorkflowId { get; set; }
    }
}
