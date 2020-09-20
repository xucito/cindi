using Cindi.Application.Results;
using Cindi.Domain.Entities.ExecutionTemplates;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.ExecutionTemplates.Commands.CreateExecutionTemplate
{
    public class CreateExecutionTemplateCommand : IRequest<CommandResult<ExecutionTemplate>>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<string, object> Inputs { get; set; }
        public string ExecutionTemplateType { get; set; }
        public string ReferenceId { get; set; }
        public string CreatedBy { get; set; }
    }
}
