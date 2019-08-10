using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.WorkflowTemplates.ValueObjects
{
    public class ConditionValidation
    {
        public bool IsValid { get; set; }
        public string Reason { get; set; }
    }
}
