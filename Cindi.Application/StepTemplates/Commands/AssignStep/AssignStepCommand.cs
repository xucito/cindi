using Cindi.Domain.Entities.Steps;
using Cindi.Domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.StepTemplates.Commands.AssignStep
{
    public class AssignStepCommand: IRequest<Step>
    {
        public string[] CompatibleDefinitions { get; set; }
    }
}
