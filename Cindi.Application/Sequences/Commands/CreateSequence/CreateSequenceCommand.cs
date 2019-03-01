using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Sequences.Commands.CreateSequence
{
    public class CreateSequenceCommand: IRequest<CommandResult>
    {
        public string Name { get; set; }
        public string SequenceTemplateId { get; set; }
        public Dictionary<string, object> Inputs { get; set; }
    }
}
