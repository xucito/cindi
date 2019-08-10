using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.WorkflowTemplates.ValueObjects
{
    public class LogicBlock
    {
        public LogicBlock()
        {
            Prerequisites = new ConditionGroup();
            SubsequentSteps = new List<SubsequentStep>();
        }

        public int Id { get; set; }
        public new ConditionGroup Prerequisites { get; set; }
        public new List<SubsequentStep> SubsequentSteps { get; set; }
    }
}
