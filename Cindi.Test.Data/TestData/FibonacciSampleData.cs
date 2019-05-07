using Cindi.Domain.Entities.Sequences;
using Cindi.Domain.Entities.SequencesTemplates;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Enums;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;
using static Cindi.Domain.ValueObjects.DynamicDataDescription;

namespace Cindi.Test.Global.TestData
{
    public static class FibonacciSampleData
    {
        public static readonly StepTemplate StepTemplate = new StepTemplate(
            "Fibonacci_stepTemplate:0",
            "",
            false,
            new Dictionary<string, DynamicDataDescription>(){
                        {"n-2", new DynamicDataDescription(){
                            Type = InputDataTypes.Int,
                            Description = ""
                        }
                    },
                        {"n-1", new DynamicDataDescription(){
                            Type = InputDataTypes.Int,
                            Description = ""
                        }}
                    },
            new Dictionary<string, DynamicDataDescription>()
                    {
                         {"n", new DynamicDataDescription(){
                            Type = InputDataTypes.Int,
                            Description = ""
                        }},
                    },
            "admin",
            DateTime.UtcNow

            )
        {
        };

        public static readonly SequenceTemplate SequenceTemplate = new SequenceTemplate(
            "Fibonacci:0",
            "",
            new Dictionary<string, DynamicDataDescription>(),
            new List<LogicBlock>()
            {
                new LogicBlock()
                {
                    Id = 0,
                    Condition = "OR",
                    PrerequisiteSteps = new List<PrerequisiteStep>
                    {
                    },
                    SubsequentSteps = new List<SubsequentStep> {
                         new SubsequentStep(){
                             StepTemplateId =StepTemplate.Id,
                             StepRefId = 0,
                                      Mappings = new List<Mapping>(){
                                      new Mapping()
                                       {
                                           DefaultValue = new DefaultValue(){
                                               Value = 1
                                           },
                                           StepInputId = "n-1"
                                       },
                                      new Mapping(){
                                           DefaultValue = new DefaultValue(){
                                               Value = 1
                                           },
                                           StepInputId = "n-2"
                                       }
                                   }
                         } }
                },
                new LogicBlock()
                {
                    Id = 1,
                    Condition = "AND",
                    PrerequisiteSteps = new List<PrerequisiteStep>
                    {
                        new PrerequisiteStep()
                        {
                            StepRefId = 0,
                            Status = StepStatuses.Successful,
                            StatusCode = 0
                        }
                    },
                    SubsequentSteps = new List<SubsequentStep> {
                         new SubsequentStep(){
                             StepTemplateId =StepTemplate.Id,
                             StepRefId = 1,
                                      Mappings = new List<Mapping>(){
                                      new Mapping()
                                       {
                                          OutputReferences = new StepOutputReference[]
                                          {
                                              new StepOutputReference()
                                              {
                                                  StepRefId = 0,
                                                  OutputId = "n"
                                              }
                                          },
                                           DefaultValue = new DefaultValue(){
                                               Value = 1
                                           },
                                           StepInputId = "n-1"
                                       },
                                      new Mapping(){
                                            OutputReferences = new StepOutputReference[]
                                            {
                                                new StepOutputReference()
                                                {
                                                    StepRefId = 0,
                                                    OutputId = "n"
                                                }
                                            },
                                           DefaultValue = new DefaultValue(){
                                               Value = 1
                                           },
                                           StepInputId = "n-2"
                                       }
                                   }
                         } }
                }
            },
            "admin",
            DateTime.UtcNow
            )
        {
        };

        public static Step Step
        {
            get
            {
                return new Step(new Domain.Entities.JournalEntries.Journal(
new Domain.Entities.JournalEntries.JournalEntry()
{
    Updates = new List<Update>()
{
                    new Update()
                    {
                        FieldName = "id",
                        Value = Guid.NewGuid(),
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "StepTemplateId",
                        Value = StepTemplate.Id,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "inputs",
                        Value = new Dictionary<string, object>()
                        {
                            {"n-2","1" },
                            {"n-1","2" }
                        },
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "createdby",
                        Value = "testUser@email.com",
                        Type = UpdateType.Create
                    },
                                        new Update()
                    {
                        FieldName = "status",
                        Value = StepStatuses.Unassigned,
                        Type = UpdateType.Create
                    }
}
}
))
                {
                };
            }
        }

        public static readonly Sequence Sequence = new Sequence(
            Guid.NewGuid(),
            SequenceTemplate.Id,
            new Dictionary<string, object>(),
            "",
            "admin",
            DateTime.UtcNow)
        {
        };

        public class FibonacciSequenceData
        {
            public SequenceTemplate sequenceTemplate;
            public SequenceTemplate sequenceTemplateWithInputs;
            public StepTemplate stepTemplate;
            public int numberOfSteps = 0;

            public FibonacciSequenceData(int numberOfSteps)
            {
                this.numberOfSteps = numberOfSteps;
                stepTemplate = FibonacciSampleData.StepTemplate;
                sequenceTemplate = FibonacciDataGenerator.GetSequenceTemplate(numberOfSteps);
                sequenceTemplateWithInputs = FibonacciDataGenerator.GetSequenceTemplateWithInputs(numberOfSteps);
            }
        }
    }
}
