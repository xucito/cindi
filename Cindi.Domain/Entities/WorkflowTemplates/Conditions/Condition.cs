using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Domain.Entities.WorkflowTemplates.ValueObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.WorkflowTemplates.Conditions
{
    [Serializable]
    [JsonConverter(typeof(ConditionSerializer))]
    public abstract class Condition
    {
        public abstract string Name { get; }
        public string Description { get; set; }
        public abstract bool Evaluate(List<Step> completedSteps);
        public abstract ConditionValidation ValidateCondition(IEnumerable<LogicBlock> validatedLogicblocks);
        public abstract bool ContainsStep(string stepName);
    }
}
