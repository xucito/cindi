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

        public static SequenceTemplate GetSequenceTemplateWithInputs(int numberOfSteps)
        {
            var stepTemplate = FibonacciSampleData.StepTemplate;

            var sequenceTemplate = new SequenceTemplate()
            {
                Id = "SimpleSequence:1"
            };

            sequenceTemplate.InputDefinitions = new Dictionary<string, DynamicDataDescription>()
            {
                {"n-1", new DynamicDataDescription()
                {
                    Type = InputDataTypes.Int
                } },{"n-2", new DynamicDataDescription()
                {
                    Type = InputDataTypes.Int
                } },
            };

            List<LogicBlock> logicBlocks = new List<LogicBlock>();

            logicBlocks.Add(new LogicBlock()
            {
                Id = 0,
                Condition = "OR",
                PrerequisiteSteps = new List<PrerequisiteStep>
                {
                },
                SubsequentSteps = new List<SubsequentStep> {
             new SubsequentStep(){
                 StepTemplateId =stepTemplate.Id,
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
                            Condition = "AND",
                            PrerequisiteSteps = new List<PrerequisiteStep> {
                                new PrerequisiteStep(){
                                   StepRefId = i,
                                   //StepTemplateReference = stepTemplate.Reference,
                                   Status = StepStatuses.Successful
                                }
                            },
                            SubsequentSteps = new List<SubsequentStep>
                            {
                                new SubsequentStep()
                                {
                                    StepTemplateId = stepTemplate.Id,
                                    StepRefId = i + 1, // This will create the next step
                                    Mappings = mappings
                                }
                            }
                        });
            }

            sequenceTemplate.LogicBlocks = logicBlocks;

            return sequenceTemplate;
        }

        public static SequenceTemplate GetSequenceTemplate(int numberOfSteps)
        {
            var stepTemplate = FibonacciSampleData.StepTemplate;

            var sequenceTemplate = new SequenceTemplate()
            {
                Id = "SimpleSequence:1"
            };

            List<LogicBlock> logicBlocks = new List<LogicBlock>();

            logicBlocks.Add(new LogicBlock()
            {
                Id = 0,
                Condition = "OR",
                PrerequisiteSteps = new List<PrerequisiteStep>
                {
                },
                SubsequentSteps = new List<SubsequentStep> {
             new SubsequentStep(){
                 StepTemplateId =stepTemplate.Id,
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
                            Condition = "AND",
                            PrerequisiteSteps = new List<PrerequisiteStep> {
                                new PrerequisiteStep(){
                                   StepRefId = i,
                                   //StepTemplateReference = stepTemplate.Reference,
                                   Status = StepStatuses.Successful
                                }
                            },
                            SubsequentSteps = new List<SubsequentStep>
                            {
                                new SubsequentStep()
                                {
                                    StepTemplateId = stepTemplate.Id,
                                    StepRefId = i + 1, // This will create the next step
                                    Mappings = mappings
                                }
                            }
                        });
            }

            sequenceTemplate.LogicBlocks = logicBlocks;

            return sequenceTemplate;
        }
    }
}
