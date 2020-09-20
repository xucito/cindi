using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.ExecutionTemplates
{
    public class ExecutionTemplate : TrackedEntity
    {
        public string Name { get; set; }
        public Dictionary<string, object> Inputs { get; set; }
        public string ReferenceId { get; set; }
        public string ExecutionTemplateType { get; set; }
        public bool IsDisabled { get; set; }
        public string Description { get; set; }

        public ExecutionTemplate()
        {
            ShardType = nameof(ExecutionTemplate);
        }
    }
}
