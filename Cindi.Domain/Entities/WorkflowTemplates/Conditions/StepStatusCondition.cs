using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Domain.Entities.WorkflowTemplates.ValueObjects;
using Cindi.Domain.Exceptions.Steps;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cindi.Domain.Entities.WorkflowTemplates.Conditions
{
    [Serializable]
    [JsonConverter(typeof(ConditionSerializer))]
    public class StepStatusCondition : Condition
    {
        public override string Name => "StepStatus"; 
        public string Comparer { get; set; } = StepStatusConditionComparers.IS;
        /// <summary>
        /// Unique Id of the Step defined within the workflow
        /// </summary>
        public string StepName { get; set; }

        public string StepTemplateReferenceId { get; set; }

        private string _status { get; set; }

        public string Status
        {
            get { return _status; }
            set
            {
                if (StepStatuses.IsValid(value))
                {
                    _status = value;
                }
                else
                {
                    throw new InvalidStepStatusInputException();
                }

            }
        }

        //You can optionally also compare status codes
        public int? StatusCode { get; set; }

        public override bool Evaluate(List<Step> completedSteps)
        {
            var foundSteps = completedSteps.Where(cs => cs.Name == StepName);
            if (foundSteps.Count() == 1)
            {
                if (foundSteps.First().Status == Status)
                {
                    if (StatusCode == null)
                    {
                        return (Comparer == StepStatusConditionComparers.IS);
                    }
                    else if (StatusCode.Value == foundSteps.First().StatusCode)
                    {
                        return (Comparer == StepStatusConditionComparers.IS);
                    }
                    return (Comparer == StepStatusConditionComparers.ISNOT);
                }
                return (Comparer == StepStatusConditionComparers.ISNOT);
            }
            // if there are no steps compared, is not is never evaluated
            return false;
        }

        public override ConditionValidation ValidateCondition(IEnumerable<LogicBlock> validatedLogicblocks)
        {
            foreach (var logicBlock in validatedLogicblocks)
            {
                if (logicBlock.SubsequentSteps.Keys.Contains(StepName))
                {
                    return new ConditionValidation()
                    {
                        IsValid = true,
                        Reason = "Step with workflowStepId " + StepName + " exists."
                    };
                }
            }

            return new ConditionValidation()
            {
                IsValid = false,
                Reason = "Step with workflowStepId " + StepName + " does not exist or complete before this step."
            };
        }
    }

    public static class StepStatusConditionComparers
    {
        public static string IS = "is";
        public static string ISNOT = "isnot";
    }
}
