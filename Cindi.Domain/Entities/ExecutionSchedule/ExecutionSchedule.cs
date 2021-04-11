using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.ExecutionSchedule
{
    public class ExecutionSchedule : TrackedEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ExecutionTemplateName {get;set;}
        public bool IsDisabled { get; set; }
        /// <summary>
        /// Enable steps to be processed in parrallel
        /// </summary>
        public bool EnableConcurrent { get; set; }
        /// <summary>
        /// When to consider steps timed out for the execution schedule
        /// </summary>
        public int TimeoutMs { get; set; }
        public DateTime NextRun { get; set; }
        /// <summary>
        /// Cron based scheduling
        /// If multiple schedules are given, the next valid time is used
        /// </summary>
        public string[] Schedule { get; set; }
    }
}
