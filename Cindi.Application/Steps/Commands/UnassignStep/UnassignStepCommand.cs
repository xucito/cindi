using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Steps.Commands.UnassignStep
{
    public class UnassignStepCommand : IRequest<CommandResult>
    {
        public Guid StepId { get; set; }
        public string CreatedBy { get; set; }
    }
}
