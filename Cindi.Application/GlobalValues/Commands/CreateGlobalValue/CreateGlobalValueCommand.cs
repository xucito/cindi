using Cindi.Application.Results;
using Cindi.Domain.Entities.GlobalValues;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.GlobalValues.Commands.CreateGlobalValue
{
    public class CreateGlobalValueCommand: IRequest<CommandResult<GlobalValue>>
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public object Value { get; set; }
        public string CreatedBy { get; set; }
    }
}
