using Cindi.Application.Results;
using Cindi.Domain.Entities.Steps;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Steps.Commands.AssignStep
{
    public class AssignStepCommand : IRequest<EncryptedCommandResult<Step>>
    {
        public string[] StepTemplateIds;
        public Guid BotId { get; set; }
    }
}
