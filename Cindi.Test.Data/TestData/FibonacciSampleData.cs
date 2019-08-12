using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Enums;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;
using static Cindi.Domain.ValueObjects.DynamicDataDescription;
using Cindi.Domain.Entities.WorkflowTemplates.Conditions;
using Cindi.Domain.Entities.WorkflowTemplates.ValueObjects;

namespace Cindi.Test.Global.TestData
{
    public static class FibonacciSampleData
    {
        public static readonly StepTemplate StepTemplate = new StepTemplate(Guid.NewGuid(),
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

        public static readonly WorkflowTemplate WorkflowTemplate = new WorkflowTemplate(Guid.NewGuid(),
            "Fibonacci:0",
            "",
            new Dictionary<string, DynamicDataDescription>(),
            new List<LogicBlock>()
            {
                new LogicBlock()
                {
                    Id = 0,
                    Prerequisites = new ConditionGroup
                    {
                        Operator = "OR",
                        Conditions = new List<Condition>(){},
                    },
                    SubsequentSteps = new List<SubsequentStep> {
                         new SubsequentStep(){
                             StepTemplateId =StepTemplate.ReferenceId,
                             WorkflowStepId = 0,
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
                    Prerequisites = new ConditionGroup
                    {
                        Operator = "AND",
                        Conditions = new List<Condition>(){ new StepStatusCondition()
                        {
                    WorkflowStepId = 0,
                    Status = StepStatuses.Successful,
                    StatusCode = 0
                        }},
                    },
                    SubsequentSteps = new List<SubsequentStep> {
                         new SubsequentStep(){
                             StepTemplateId =StepTemplate.ReferenceId,
                             WorkflowStepId = 1,
                                      Mappings = new List<Mapping>(){
                                      new Mapping()
                                       {
                                          OutputReferences = new StepOutputReference[]
                                          {
                                              new StepOutputReference()
                                              {
                                                  WorkflowStepId = 0,
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
                                                    WorkflowStepId = 0,
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
                        FieldName = "steptemplateid",
                        Value = StepTemplate.ReferenceId,
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

        public static readonly Workflow Workflow = new Workflow(
            Guid.NewGuid(),
            WorkflowTemplate.ReferenceId,
            new Dictionary<string, object>(),
            "",
            "admin",
            DateTime.UtcNow)
        {
        };

        public class FibonacciWorkflowData
        {
            public WorkflowTemplate workflowTemplate;
            public WorkflowTemplate workflowTemplateWithInputs;
            public StepTemplate stepTemplate;
            public int numberOfSteps = 0;

            public FibonacciWorkflowData(int numberOfSteps)
            {
                this.numberOfSteps = numberOfSteps;
                stepTemplate = FibonacciSampleData.StepTemplate;
                workflowTemplate = FibonacciDataGenerator.GetWorkflowTemplate(numberOfSteps);
                workflowTemplateWithInputs = FibonacciDataGenerator.GetWorkflowTemplateWithInputs(numberOfSteps);
            }
        }
    }
}
