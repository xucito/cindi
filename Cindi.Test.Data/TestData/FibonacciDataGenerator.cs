using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Enums;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;
using static Cindi.Domain.ValueObjects.DynamicDataDescription;
using Cindi.Domain.Entities.WorkflowTemplates.ValueObjects;
using Cindi.Domain.Entities.WorkflowTemplates.Conditions;

namespace Cindi.Test.Global.TestData
{
    public static class FibonacciDataGenerator
    {

        public static Step EvaluateFibonacci(Step step)
        {
            /*step.Outputs = new Dictionary<string, object>()
            {
                {"", }
                new CommonData("n",(int)InputDataType.Int,""+(int.Parse(CindiUtility.GetData(step.Inputs, "n-1").Value) + int.Parse(CindiUtility.GetData(step.Inputs, "n-2").Value)))
            };*/

            return step;
        }

        public static WorkflowTemplate GetWorkflowTemplateWithInputs(int numberOfSteps)
        {
            var stepTemplate = FibonacciSampleData.StepTemplate;



            Dictionary<string, LogicBlock> logicBlocks = new Dictionary<string, LogicBlock>();

            logicBlocks.Add("0", new LogicBlock()
            {
                Dependencies = new ConditionGroup
                {
                },
                SubsequentSteps = new Dictionary<string, SubsequentStep> {
                    {"0",
             new SubsequentStep(){
                 StepTemplateId =stepTemplate.ReferenceId,
                          Mappings = new Dictionary<string, Mapping>(){
                                  {"n-1",
                          new Mapping()
                           {
                               DefaultValue = new DefaultValue(){
                                   Value = 1
                               }
                           } },
                              {  "n-2",
                          new Mapping(){
                               DefaultValue = new DefaultValue(){
                                   Value = 1
                               }
                          }
                              }
                           }
                       }
             } }
            });

            for (var i = 0; i < numberOfSteps - 1; i++)
            {

                var mappings = new Dictionary<string, Mapping>()
                {
                    {"n-2",
                  new Mapping()
                {
                    OutputReferences = new StepOutputReference[]
                    {
                        new StepOutputReference()
                        {
                            StepName =  ""+i,
                            OutputId = "n"
                        }
                    }
                }
                    }
                };

                if (i == 0)
                {
                    mappings.Add("n-1", new Mapping()
                    {
                        DefaultValue = new DefaultValue()
                        {
                            Value = 1
                        }
                    });
                }
                else
                {

                    mappings.Add("n-1", new Mapping()
                    {

                        OutputReferences = new StepOutputReference[]
                                            {
                                                new StepOutputReference()
                                                {
                                                    StepName = ""+(i -1),
                                                    OutputId = "n"
                                                }
                                            }
                    });
                }


                logicBlocks.Add((i + 2) + "",
                        new LogicBlock()
                        {
                            Dependencies = new ConditionGroup
                            {
                                Operator = "AND",
                                Conditions = new Dictionary<string, Condition>(){
                                    {"0", new StepStatusCondition()
                                {
                                    StepName = ""+i,
                                    Status = StepStatuses.Successful
                                }} }
                            },
                            SubsequentSteps = new Dictionary<string, SubsequentStep>
                            {
                                {""+(i + 1),
                                new SubsequentStep()
                                {
                                    StepTemplateId = stepTemplate.ReferenceId,
                                    Mappings = mappings
                                }
                            }
                                }
                        });
            }

            var workflowTemplate = new WorkflowTemplate()
            {
                Id = Guid.NewGuid(),
                ReferenceId = "SimpleSequence:1",
                InputDefinitions = new Dictionary<string, DynamicDataDescription>()
                                        {
                                            {"n-1", new DynamicDataDescription()
                                            {
                                                Type = InputDataTypes.Int
                                            } },{"n-2", new DynamicDataDescription()
                                            {
                                                Type = InputDataTypes.Int
                                            } },
                                        },
                LogicBlocks = logicBlocks
            };

            return workflowTemplate;
        }

        public static WorkflowTemplate GetWorkflowTemplate(int numberOfSteps)
        {
            var stepTemplate = FibonacciSampleData.StepTemplate;

            Dictionary<string, LogicBlock> logicBlocks = new Dictionary<string, LogicBlock>();

            logicBlocks.Add("1", new LogicBlock()
            {
                Dependencies = new ConditionGroup
                {
                },
                SubsequentSteps = new Dictionary<string, SubsequentStep> {
                    { "0",
             new SubsequentStep(){
                 StepTemplateId =stepTemplate.ReferenceId,
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
                       }
             } }
            });

            for (var i = 0; i < numberOfSteps - 1; i++)
            {

                var mappings = new Dictionary<string, Mapping>()
                { {"n-2",
                  new Mapping()
                {
                    OutputReferences = new StepOutputReference[]
                    {
                        new StepOutputReference()
                        {
                            StepName =  ""+i,
                            OutputId = "n"
                        }
                    }
                  }
                }
                };

                if (i == 0)
                {
                    mappings.Add("n-1", new Mapping()
                    {
                        DefaultValue = new DefaultValue()
                        {
                            Value = 1
                        }
                    });
                }
                else
                {

                    mappings.Add("n-1", new Mapping()
                    {
                        OutputReferences = new StepOutputReference[]
                                            {
                                                new StepOutputReference()
                                                {
                                                    StepName = "" + (i -1),
                                                    OutputId = "n"
                                                }
                                            }
                    });
                }


                logicBlocks.Add("" + (i + 2),
                        new LogicBlock()
                        {
                            Dependencies = new ConditionGroup
                            {
                                Operator = "AND",
                                Conditions = new Dictionary<string, Condition>(){
                                    {"0", new StepStatusCondition()
                                {
                                    StepName =""+ i,
                                    Status = StepStatuses.Successful
                                }} }
                            },
                            SubsequentSteps = new Dictionary<string, SubsequentStep>
                            {
                                { ""+(i + 1),
                                new SubsequentStep()
                                {
                                    StepTemplateId = stepTemplate.ReferenceId,
                                    Mappings = mappings
                                }
                            }
                            }
                        });
            }

            var workflowTemplate = new WorkflowTemplate()
            {
                ReferenceId = "SimpleSequence:1",
                InputDefinitions = new Dictionary<string, DynamicDataDescription>()
                                        {
                                            {"n-1", new DynamicDataDescription()
                                            {
                                                Type = InputDataTypes.Int
                                            } },{"n-2", new DynamicDataDescription()
                                            {
                                                Type = InputDataTypes.Int
                                            } },
                                        },
                LogicBlocks = logicBlocks,
            };

            return workflowTemplate;
        }
    }
}
