using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Workflows.Commands.CreateWorkflow
{
    public class CreateWorkflowCommand: IRequest<CommandResult>
    {
        public string Name { get; set; }
        public string WorkflowTemplateId { get; set; }
        //Default to an empty dictionary
        public Dictionary<string, object> Inputs { get; set; } = new Dictionary<string, object>();
        public string CreatedBy { get; set; }
    }
}
