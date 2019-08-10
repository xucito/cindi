using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.WorkflowTemplates.ValueObjects
{
    public class ConditionGroupValidation
    {
        public bool IsValid { get; set; }
        public List<ConditionValidation> ConditionsValidation { get; set; }
        public List<ConditionGroupValidation> ConditionGroupsValidation { get; set; }
        public string Reason { get; set; }
    }
}
