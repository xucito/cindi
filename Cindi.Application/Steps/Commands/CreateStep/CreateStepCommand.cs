using Cindi.Application.Results;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Cindi.Application.Steps.Commands.CreateStep
{
    public class CreateStepCommand: IRequest<CommandResult<Step>>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Maps to a Step Definition
        /// </summary>
        //[Required]
        public string StepTemplateId { get; set; }

        /// <summary>
        /// Input for the task, the Input name is the dictionary key and the input value is the Dictionary value
        /// </summary>
        public Dictionary<string, object> Inputs { get; set; }

        public List<string> Tests { get; set; }

        public string CreatedBy { get; set; }

        public int? StepRefId { get; set; }
        public Guid? SequenceId { get; set; }

    }
}
