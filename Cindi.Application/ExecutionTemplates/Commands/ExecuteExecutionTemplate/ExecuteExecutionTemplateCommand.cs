using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.ExecutionTemplates.Commands.ExecuteExecutionTemplate
{
    public class ExecuteExecutionTemplateCommand : IRequest<CommandResult>
    {
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public Guid? ExecutionScheduleId { get; set; }
    }
}
