using Cindi.Application.Results;
using Cindi.Domain.Entities.Steps;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Steps.Commands.CancelStep
{
    public class CancelStepCommand : IRequest<CommandResult>
    {
        public Guid StepId { get; set; }
        public string CreatedBy { get; set; }
    }
}
