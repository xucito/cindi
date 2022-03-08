using Cindi.Application.Results;
using Cindi.Domain.Entities.Steps;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Steps.Commands.SuspendStep
{
    public class SuspendStepCommand : IRequest<CommandResult>
    {
        public Guid StepId { get; set; }
        public DateTimeOffset? SuspendedUntil { get; set; }
        public string CreatedBy { get; set; }
    }
}
