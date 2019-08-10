using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.WorkflowTemplates.Conditions;
using Cindi.Domain.Entities.WorkflowTemplates.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Test.Global.TestClasses
{
    public class AlwaysTrueCondition : Condition
    {
        public override bool Evaluate(List<Step> completedSteps)
        {
            return true;
        }

        public override ConditionValidation ValidateCondition(IEnumerable<LogicBlock> validatedLogicblocks)
        {
            return new ConditionValidation()
            {
                IsValid = true,
                Reason = "This function will always be valid"
            };
        }
    }
}
