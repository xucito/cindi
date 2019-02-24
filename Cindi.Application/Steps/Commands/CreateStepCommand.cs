﻿using Cindi.Application.Results;
using Cindi.Domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Cindi.Application.Steps.Commands
{
    public class CreateStepCommand: IRequest<CommandResult>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Maps to a Step Definition
        /// </summary>
        //[Required]
        public TemplateReference TemplateReference { get; set; }

        /// <summary>
        /// Input for the task, the Input name is the dictionary key and the input value is the Dictionary value
        /// </summary>
        public Dictionary<string, object> Inputs { get; set; }

        public List<TemplateReference> Tests { get; set; }
    }
}
