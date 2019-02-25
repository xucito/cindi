using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Steps.Commands.AssignStep
{
    public class AssignStepCommand : IRequest<CommandResult>
    {
        public string[] CompatibleStepTemplateIds;
    }
}
