using Cindi.Domain.Entities.WorkflowTemplates.Conditions;
using Cindi.Domain.Entities.WorkflowTemplates.ValueObjects;
using Cindi.Test.Global.TestData;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Cindi.Domain.Tests.WorkflowTemplates.Conditions
{
    public class ConditionGroup_Tests
    {
        [Fact]
        public void EvaluateConditionGroups()
        {
            Assert.True(ConditionSampleData.OneLayerTrueConditionGroup.Evaluate(new List<Entities.Steps.Step>()));
            Assert.True(ConditionSampleData.OneLayerORTrueConditionGroup.Evaluate(new List<Entities.Steps.Step>()));
            Assert.False(ConditionSampleData.OneLayerFalseConditionGroup.Evaluate(new List<Entities.Steps.Step>()));
            Assert.True(ConditionSampleData.TwoLayerTrueConditionGroup.Evaluate(new List<Entities.Steps.Step>()));
            Assert.False(ConditionSampleData.TwoLayerFalseConditionGroup.Evaluate(new List<Entities.Steps.Step>()));
            Assert.True(ConditionSampleData.ThreeLayerTrueConditionGroup.Evaluate(new List<Entities.Steps.Step>()));
            Assert.False(ConditionSampleData.ThreeLayerFalseConditionGroup.Evaluate(new List<Entities.Steps.Step>()));
        }
    }
}
