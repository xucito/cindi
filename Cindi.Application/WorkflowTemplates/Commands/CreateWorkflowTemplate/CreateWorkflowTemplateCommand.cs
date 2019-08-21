using Cindi.Application.Results;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Domain.Entities.WorkflowTemplates.ValueObjects;
using Cindi.Domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Cindi.Application.WorkflowTemplates.Commands.CreateWorkflowTemplate
{
    public class CreateWorkflowTemplateCommand: IRequest<CommandResult>
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Version { get; set; }
        public string Description { get; set; }
        public Dictionary<string, LogicBlock> LogicBlocks { get; set; } = new Dictionary<string, LogicBlock>();
        public Dictionary<string, DynamicDataDescription> InputDefinitions { get; set; } = new Dictionary<string, DynamicDataDescription>();
        public string CreatedBy { get; set; }
    }
}
