using Cindi.Application.Results;
using Cindi.Domain.Entities.SequencesTemplates;
using Cindi.Domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Cindi.Application.SequenceTemplates.Commands.CreateSequenceTemplate
{
    public class CreateSequenceTemplateCommand: IRequest<CommandResult>
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Version { get; set; }
        public string Description { get; set; }
        public List<LogicBlock> LogicBlocks { get; set; }
        public Dictionary<string, DynamicDataDescription> InputDefinitions { get; set; }
    }
}
