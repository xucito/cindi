using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Cindi.Domain.Entities.ExecutionTemplates
{
    public class ExecutionTemplate: TrackedEntity
    {
        public string Name { get; set; }
        public Dictionary<string, object> Inputs { get; set; }
        public string ReferenceId { get; set; }
        public string ExecutionTemplateType { get; set; }
        public bool IsDisabled { get; set; }
        public string Description { get; set; }
    }
}
