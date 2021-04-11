using Cindi.Domain.Entities;
using Cindi.Domain.Entities.Metrics;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.WorkflowTemplates.Conditions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Cindi.Domain.Tests.Utilities
{
    public class Serializer_Tests
    {
        [Fact]
        public void Serialize_StepStatus()
        {
            var converted = JsonConvert.SerializeObject(new StepStatusCondition()
            {
                Comparer = StepStatusConditionComparers.IS,
                Status = StepStatuses.Successful,
                StepName = "0"
            });

            var unconverted = JsonConvert.DeserializeObject<Condition>(converted);
        }

        [Fact]
        public void TestConverstion_Test()
        {
            Assert.True(typeof(TrackedEntity).IsAssignableFrom(typeof(Step)));
            Assert.False(typeof(TrackedEntity).IsAssignableFrom(typeof(MetricTick)));
        }
    }
}
