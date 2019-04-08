using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Steps.Commands.AppendStepLog
{
    public class AppendStepLogCommand: IRequest<CommandResult>
    {
        public Guid StepId { get; set; }
        public string Log { get; set; }
        public string CreatedBy { get; set; }
    }
}
