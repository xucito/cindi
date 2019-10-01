using Cindi.Application.Results;
using Cindi.Domain.Entities.GlobalValues;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.GlobalValues.Commands.UpdateGlobalValue
{
    public class UpdateGlobalValueCommand : IRequest<CommandResult<GlobalValue>>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public object Value { get; set; }
        public string CreatedBy { get; set; }
    }
}
