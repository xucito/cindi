using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.WorkflowTemplates.Conditions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Cindi.Domain.Entities.WorkflowTemplates.ValueObjects
{
    public class ConditionGroup
    {
        [Required]
        public string Operator { get; set; } = OperatorStatements.AND;
        public Dictionary<string, Condition> Conditions { get; set; } = new Dictionary<string, Condition>();
        public Dictionary<string, ConditionGroup> ConditionGroups { get; set; } = new Dictionary<string, ConditionGroup>();

        public bool ContainsStep(string stepName)
        {
            foreach(var condition in Conditions)
            {
                if(condition.Value.ContainsStep(stepName))
                {
                    return true;
                }
            }

            foreach (var conditionGroup in ConditionGroups)
            {
                if (conditionGroup.Value.ContainsStep(stepName))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// A workflow's logic should always be based on historic steps
        /// </summary>
        /// <param name="completedSteps"></param>
        /// <returns></returns>
        public bool Evaluate(List<Step> completedSteps)
        {
            foreach (var condition in Conditions)
            {
                var evalationResult = condition.Value.Evaluate(completedSteps);
                if (!evalationResult && Operator == OperatorStatements.AND)
                {
                    return false;
                }

                if (evalationResult && Operator == OperatorStatements.OR)
                {
                    return true;
                }
            }

            foreach (var conditionGroup in ConditionGroups)
            {
                var evalationResult = conditionGroup.Value.Evaluate(completedSteps);
                if (!evalationResult && Operator == OperatorStatements.AND)
                {
                    return false;
                }

                if (evalationResult && Operator == OperatorStatements.OR)
                {
                    return true;
                }
            }

            // If none of the conditions have returned false, by the default the Evaluation must be true or if the operator or is used with no conditions
            if (Operator == OperatorStatements.AND || (OperatorStatements.OR == Operator && ConditionGroups.Count() == 0 && Conditions.Count() == 0))
            {
                return true;
            }
            // If none of the "OR" have not been hit, you will return true
            else
            {
                return false;
            }
        }

        public ConditionGroupValidation ValidateConditionGroup(IEnumerable<LogicBlock> validatedLogicBlock)
        {
            var finalConditionGroup = new ConditionGroupValidation()
            {
                ConditionGroupsValidation = new List<ConditionGroupValidation>(),
                ConditionsValidation = new List<ConditionValidation>()
            };

            bool isValid = true;
            foreach (var condition in Conditions)
            {
                var valuation = condition.Value.ValidateCondition(validatedLogicBlock);
                if (!valuation.IsValid)
                    isValid = false;
                finalConditionGroup.ConditionsValidation.Add(valuation);
            }

            foreach (var conditionGroup in ConditionGroups)
            {
                var valuation = conditionGroup.Value.ValidateConditionGroup(validatedLogicBlock);
                if (!valuation.IsValid)
                    isValid = false;
                finalConditionGroup.ConditionGroupsValidation.Add(valuation);
            }
            finalConditionGroup.IsValid = isValid;
            return finalConditionGroup;
        }
    }
}
