using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.WorkflowTemplates.ValueObjects
{
    public class LogicBlock
    {
        public LogicBlock()
        {
            Dependencies = new ConditionGroup();
            SubsequentSteps = new Dictionary<string, SubsequentStep>();
        }
        public new ConditionGroup Dependencies { get; set; }
        public new Dictionary<string, SubsequentStep> SubsequentSteps { get; set; }
    }
}
