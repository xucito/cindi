using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Enums;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Test.Global.TestData
{
    public static class SecretSampleData
    {
        public static readonly StepTemplate StepTemplate = new StepTemplate(
            "Pass_Password:0",
            "",
            false,
            new Dictionary<string, DynamicDataDescription>(){

                        {"secret", new DynamicDataDescription(){
                            Type = InputDataTypes.Secret,
                            Description = ""
                        }}
                    },
            new Dictionary<string, DynamicDataDescription>()
                    {
                         {"secret", new DynamicDataDescription(){
                            Type = InputDataTypes.Secret,
                            Description = ""
                        }},
                    },
            "admin",
            DateTime.UtcNow

            )
        { };
    }
}
