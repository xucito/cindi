using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.ExecutionSchedules.Commands.RecalculateExecutionSchedule
{
    public class RecalculateExecutionScheduleCommand : IRequest<CommandResult>
    {
        public string Name { get; set; }
    }
}
