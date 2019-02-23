using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;
using static Cindi.Domain.ValueObjects.DynamicDataDescription;

namespace Cindi.Infrastructure.Tests.TestData
{
    public static class FibonacciSampleData
    {
        public static readonly StepTemplate StepTemplate = new StepTemplate()
        {
            Name = "Fibonacci_stepTemplate",
            Version = "0",
            InputDefinitions = new Dictionary<string, DynamicDataDescription>(){
                        {"n-2", new DynamicDataDescription(){
                            Type = (int)InputDataType.Int,
                            Description = ""
                        }
                    },
                        {"n-1", new DynamicDataDescription(){
                            Type = (int)InputDataType.Int,
                            Description = ""
                        }}
                    },
            OutputDefinitions = new Dictionary<string, DynamicDataDescription>()
                    {
                         {"n", new DynamicDataDescription(){
                            Type = (int)InputDataType.Int,
                            Description = ""
                        }},
                    }
        };

        public static readonly Step Step = new Step()
        {
            StepTemplateReference = StepTemplate.Reference,
            Inputs = new Dictionary<string, object>()
            {
                {"n-2","1" },
                {"n-1","2" }
            }
        };
    }
}
