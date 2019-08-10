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



            List<LogicBlock> logicBlocks = new List<LogicBlock>();

            logicBlocks.Add(new LogicBlock()
            {
                Id = 0,
                Prerequisites = new ConditionGroup
                {
                },
                SubsequentSteps = new List<SubsequentStep> {
             new SubsequentStep(){
                 StepTemplateId =stepTemplate.ReferenceId,
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
            });

            for (var i = 0; i < numberOfSteps - 1; i++)
            {

                var mappings = new List<Mapping>()
                {
                  new Mapping()
                {
                    OutputReferences = new StepOutputReference[]
                    {
                        new StepOutputReference()
                        {
                            StepRefId =  i,
                            OutputId = "n"
                        }
                    },
                    StepInputId = "n-2"
                }
                };

                if (i == 0)
                {
                    mappings.Add(new Mapping()
                    {
                        DefaultValue = new DefaultValue()
                        {
                            Value = 1
                        },
                        StepInputId = "n-1"
                    });
                }
                else
                {

                    mappings.Add(new Mapping()
                    {
                        OutputReferences = new StepOutputReference[]
                                            {
                                                new StepOutputReference()
                                                {
                                                    StepRefId = i -1,
                                                    OutputId = "n"
                                                }
                                            },
                        StepInputId = "n-1"
                    });
                }


                logicBlocks.Add(
                        new LogicBlock()
                        {
                            Id = 0,
                            Prerequisites = new ConditionGroup
                            {
                                Operator = "AND",
                                Conditions = new List<Condition>(){ new StepStatusCondition()
                                {
                                    StepRefId = i,
                                    Status = StepStatuses.Successful
                                }},
                            },
                            SubsequentSteps = new List<SubsequentStep>
                            {
                                new SubsequentStep()
                                {
                                    StepTemplateId = stepTemplate.ReferenceId,
                                    StepRefId = i + 1, // This will create the next step
                                    Mappings = mappings
                                }
                            }
                        });
            }

            var workflowTemplate = new WorkflowTemplate(
                new Journal(new JournalEntry
                {
                    Updates = new List<Update> {
                                    new Update()
                                    {
                                        Value = Guid.NewGuid(),
                                        Type = UpdateType.Create,
                                        FieldName = "id"
                                    },
                                    new Update()
                                    {
                                        Value = "SimpleSequence:1",
                                        Type = UpdateType.Create,
                                        FieldName = "referenceid"
                                    },
                                    new Update()
                                    {
                                        Value = new Dictionary<string, DynamicDataDescription>()
                                        {
                                            {"n-1", new DynamicDataDescription()
                                            {
                                                Type = InputDataTypes.Int
                                            } },{"n-2", new DynamicDataDescription()
                                            {
                                                Type = InputDataTypes.Int
                                            } },
                                        },
                                        Type = UpdateType.Create,
                                        FieldName = "inputdefinitions"
                                    },
                                    new Update()
                                    {
                                        Value = logicBlocks,
                                        Type = UpdateType.Create,
                                        FieldName = "logicblocks"
                                    }
                    }
                }))
            {
            };

            return workflowTemplate;
        }

        public static WorkflowTemplate GetWorkflowTemplate(int numberOfSteps)
        {
            var stepTemplate = FibonacciSampleData.StepTemplate;

            List<LogicBlock> logicBlocks = new List<LogicBlock>();

            logicBlocks.Add(new LogicBlock()
            {
                Id = 0,
                Prerequisites = new ConditionGroup
                {
                },
                SubsequentSteps = new List<SubsequentStep> {
             new SubsequentStep(){
                 StepTemplateId =stepTemplate.ReferenceId,
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
            });

            for (var i = 0; i < numberOfSteps - 1; i++)
            {

                var mappings = new List<Mapping>()
                {
                  new Mapping()
                {
                    OutputReferences = new StepOutputReference[]
                    {
                        new StepOutputReference()
                        {
                            StepRefId =  i,
                            OutputId = "n"
                        }
                    },
                    StepInputId = "n-2"
                }
                };

                if (i == 0)
                {
                    mappings.Add(new Mapping()
                    {
                        DefaultValue = new DefaultValue()
                        {
                            Value = 1
                        },
                        StepInputId = "n-1"
                    });
                }
                else
                {

                    mappings.Add(new Mapping()
                    {
                        OutputReferences = new StepOutputReference[]
                                            {
                                                new StepOutputReference()
                                                {
                                                    StepRefId = i -1,
                                                    OutputId = "n"
                                                }
                                            },
                        StepInputId = "n-1"
                    });
                }


                logicBlocks.Add(
                        new LogicBlock()
                        {
                            Id = 0,
                            Prerequisites = new ConditionGroup
                            {
                                Operator = "AND",
                                Conditions = new List<Condition>(){ new StepStatusCondition()
                                {
                                    StepRefId = i,
                                    Status = StepStatuses.Successful
                                }},
                            },
                            SubsequentSteps = new List<SubsequentStep>
                            {
                                new SubsequentStep()
                                {
                                    StepTemplateId = stepTemplate.ReferenceId,
                                    StepRefId = i + 1, // This will create the next step
                                    Mappings = mappings
                                }
                            }
                        });
            }

            var workflowTemplate = new WorkflowTemplate(
                new Journal(new JournalEntry
                {
                    Updates = new List<Update> {
                                    new Update()
                                    {
                                        Value = "SimpleSequence:1",
                                        Type = UpdateType.Create,
                                        FieldName = "WorkflowTemplateId"
                                    },
                                    new Update()
                                    {
                                        Value = new Dictionary<string, DynamicDataDescription>()
                                        {
                                            {"n-1", new DynamicDataDescription()
                                            {
                                                Type = InputDataTypes.Int
                                            } },{"n-2", new DynamicDataDescription()
                                            {
                                                Type = InputDataTypes.Int
                                            } },
                                        },
                                        Type = UpdateType.Create,
                                        FieldName = "inputdefinitions"
                                    },
                                    new Update()
                                    {
                                        Value = logicBlocks,
                                        Type = UpdateType.Create,
                                        FieldName = "logicblocks"
                                    }
                    }
                }))
            {
            };

            return workflowTemplate;
        }
    }
}
