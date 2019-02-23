using Cindi.Domain.Entities.StepTemplates;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.StepTemplates.Queries.GetStepTemplate
{
    public class GetStepTemplateQuery : IRequest<StepTemplate>
    {
        public string Name { get; set; }
        public string Version { get; set; }
    }
}
