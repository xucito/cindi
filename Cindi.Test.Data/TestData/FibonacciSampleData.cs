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
        public static readonly StepTemplate StepTemplate = new StepTemplate()
        {
            Id = Guid.NewGuid(),
            ReferenceId = "Fibonacci_stepTemplate:0",
            InputDefinitions = new Dictionary<string, DynamicDataDescription>() {
                { "n-2", new DynamicDataDescription() {
                    Type = InputDataTypes.Int,
                    Description = ""
                }
                },
                { "n-1", new DynamicDataDescription() {
                    Type = InputDataTypes.Int,
                    Description = ""
                } }
            },
            OutputDefinitions = new Dictionary<string, DynamicDataDescription>()
            {
                { "n", new DynamicDataDescription() {
                    Type = InputDataTypes.Int,
                    Description = ""
                } },
            }
        };

        public static readonly WorkflowTemplate ConcurrentWorkflowTemplate = new WorkflowTemplate()
        {
            Id = Guid.NewGuid(),
            ReferenceId = "ConcurrentFibonacci:0",
            InputDefinitions = new Dictionary<string, DynamicDataDescription>(),
            LogicBlocks = new Dictionary<string, LogicBlock>()
            {
                {  "0",
                new LogicBlock()
                {
                    Dependencies = new ConditionGroup
                    {
                        Operator = "OR",
                        Conditions = new Dictionary<string, Condition>(){},
                    },
                    SubsequentSteps = new Dictionary<string, SubsequentStep> {
                        { ""+0,
                         new SubsequentStep(){
                             StepTemplateId =StepTemplate.ReferenceId,
                                      Mappings = new Dictionary<string, Mapping>(){
                                          {"n-1",
                                      new Mapping()
                                       {
                                           DefaultValue = new DefaultValue(){
                                               Value = 1
                                           }
                                       }
                                          },
                                      {"n-2",
                                      new Mapping(){
                                           DefaultValue = new DefaultValue(){
                                               Value = 1
                                           }
                                       }
                                          }
                                   }
                         } }
                }
                }
                },
                {  "1",
                new LogicBlock()
                {
                    Dependencies = new ConditionGroup
                    {
                        Operator = "OR",
                        Conditions = new Dictionary<string, Condition>(){},
                    },
                    SubsequentSteps = new Dictionary<string, SubsequentStep> {
                        { ""+1,
                         new SubsequentStep(){
                             StepTemplateId =StepTemplate.ReferenceId,
                                      Mappings = new Dictionary<string, Mapping>(){
                                          {"n-1",
                                      new Mapping()
                                       {
                                           DefaultValue = new DefaultValue(){
                                               Value = 1
                                           }
                                       }
                                          },
                                      {"n-2",
                                      new Mapping(){
                                           DefaultValue = new DefaultValue(){
                                               Value = 1
                                           }
                                       }
                                          }
                                   }
                         } }
                }
                }
                }
            }
        };

        public static readonly WorkflowTemplate WorkflowTemplate = new WorkflowTemplate()
        {
            Id = Guid.NewGuid(),
            ReferenceId = "Fibonacci:0",
            InputDefinitions = new Dictionary<string, DynamicDataDescription>(),
            LogicBlocks = new Dictionary<string, LogicBlock>()
            {
                {  "0",
                new LogicBlock()
                {
                    Dependencies = new ConditionGroup
                    {
                        Operator = "OR",
                        Conditions = new Dictionary<string, Condition>(){},
                    },
                    SubsequentSteps = new Dictionary<string, SubsequentStep> {
                        { ""+0,
                         new SubsequentStep(){
                             StepTemplateId =StepTemplate.ReferenceId,
                                      Mappings = new Dictionary<string, Mapping>(){
                                          {"n-1",
                                      new Mapping()
                                       {
                                           DefaultValue = new DefaultValue(){
                                               Value = 1
                                           }
                                       }
                                          },
                                      {"n-2",
                                      new Mapping(){
                                           DefaultValue = new DefaultValue(){
                                               Value = 1
                                           }
                                       }
                                          }
                                   }
                         } }
                }
                }
                },
                {
                    "1",
                new LogicBlock()
                {
                    Dependencies = new ConditionGroup
                    {
                        Operator = "AND",
                        Conditions = new Dictionary<string, Condition>()
                        {
                            { "0", new StepStatusCondition()
                        {
                    StepName = ""+0,
                    Status = StepStatuses.Successful,
                    StatusCode = 0
                        } }
                        },
                    },
                    SubsequentSteps = new Dictionary<string, SubsequentStep> {
                        { "1",
                         new SubsequentStep(){
                             StepTemplateId =StepTemplate.ReferenceId,
                                      Mappings = new Dictionary<string, Mapping>(){
                                          {"n-1",
                                      new Mapping()
                                       {
                                          OutputReferences = new StepOutputReference[]
                                          {
                                              new StepOutputReference()
                                              {
                                                  StepName = "0",
                                                  OutputId = "n"
                                              }
                                          },
                                           DefaultValue = new DefaultValue(){
                                               Value = 1
                                           }
                                       }
                                          },
                                      {"n-2",
                                      new Mapping(){
                                            OutputReferences = new StepOutputReference[]
                                            {
                                                new StepOutputReference()
                                                {
                                                    StepName = "0",
                                                    OutputId = "n"
                                                }
                                            },
                                           DefaultValue = new DefaultValue(){
                                               Value = 1
                                           }
                                       }
                                   }
                         } }
                        }
                    }
                }
                }
            }
        };

        public static Step Step
        {
            get
            {
                return new Step()
                {
                    Id = Guid.NewGuid(),
                    StepTemplateId = StepTemplate.ReferenceId,
                    Inputs = new Dictionary<string, object>()
                        {
                            {"n-2","1" },
                            {"n-1","2" }
                        }
                    ,
                    Status = StepStatuses.Unassigned
                };
            }
        }

        public static readonly Workflow Workflow = new Workflow()
        {
            Id = Guid.NewGuid(),
            WorkflowTemplateId = WorkflowTemplate.ReferenceId,
            Inputs = new Dictionary<string, object>()
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
