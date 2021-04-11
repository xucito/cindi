using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.ExecutionSchedules.Commands.UpdateExecutionSchedule
{
    public class UpdateExecutionScheduleCommand : IRequest<CommandResult>
    {
        public bool RunImmediately { get; set; }
        public bool? IsDisabled { get; set; }
        public string[] Schedule { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public bool? EnableConcurrent { get; set; }
        public int? TimeoutMs { get; set; }
    }
}
