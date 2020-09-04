﻿using Cindi.Application.Results;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Cindi.Application.StepTemplates.Commands.CreateStepTemplate
{
    public class CreateStepTemplateCommand: IRequest<CommandResult>
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Version { get; set; }
        public bool AllowDynamicInputs = false;
        public Dictionary<string, DynamicDataDescription> InputDefinitions { get; set; }
        public Dictionary<string, DynamicDataDescription> OutputDefinitions { get; set; }
        public string CreatedBy { get; set; }
        public string Description { get; set; }
    }
}