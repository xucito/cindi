using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.ExecutionSchedule
{
    public class ExecutionSchedule: TrackedEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ExecutionTemplateName {get;set;}
        public bool IsDisabled { get; set; }
        public DateTimeOffset NextRun { get; set; }
        /// <summary>
        /// Cron based scheduling
        /// If multiple schedules are given, the next valid time is used
        /// </summary>
        public string[] Schedule { get; set; }
    }
}
