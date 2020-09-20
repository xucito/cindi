using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Enums;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.InternalBots.InternalSteps
{
    public class InternalStepLibrary
    {

        public static StepTemplate GenerateSystemReport = new StepTemplate()
        {
            ReferenceId = "_GenerateSystemReport:0",
            InputDefinitions = new Dictionary<string, DynamicDataDescription>()
            {
            },
            OutputDefinitions = new Dictionary<string, DynamicDataDescription>()
            {
                {
                    "report", new DynamicDataDescription(){
                    Type = InputDataTypes.String
                }
                },
                {
                    "slack_report", new DynamicDataDescription(){
                    Type = InputDataTypes.String,
                    Description = "The outputted report in slack output format"
                }
                },
                {
                    "markdown", new DynamicDataDescription(){
                    Type = InputDataTypes.String,
                    Description = "The outputted report in markdown output format"
                }
                }
            }
        };

        public static StepTemplate SendSlackMessage = new StepTemplate()
        {
            ReferenceId = "_SendSlackMessage:0",
            InputDefinitions = new Dictionary<string, DynamicDataDescription>()
            {
               {"webhook_url", new DynamicDataDescription(){
                     Type = InputDataTypes.String
               } },
               {"from", new DynamicDataDescription(){
                     Type = InputDataTypes.String
               } },
                {"icon_emoji", new DynamicDataDescription(){
                     Type = InputDataTypes.String
               } },
                 {"icon_url", new DynamicDataDescription(){
                     Type = InputDataTypes.String
               } },
                 {"markdown", new DynamicDataDescription(){
                     Type = InputDataTypes.String
               } },
               {"channel", new DynamicDataDescription(){
                     Type = InputDataTypes.String
               } },
                {"blocks", new DynamicDataDescription(){
                     Type = InputDataTypes.String
               } },
                {"text", new DynamicDataDescription(){
                     Type = InputDataTypes.String
               } }
            },
            OutputDefinitions = new Dictionary<string, DynamicDataDescription>()
            {
            }
        };

        public static List<StepTemplate> All = new List<StepTemplate>()
        {
            GenerateSystemReport,
            SendSlackMessage
        };
    }
}
