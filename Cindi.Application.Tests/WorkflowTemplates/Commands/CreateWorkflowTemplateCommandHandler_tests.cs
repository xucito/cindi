using Cindi.Application.Interfaces;
using Cindi.Application.Workflows.Commands;
using Cindi.Application.WorkflowTemplates.Commands.CreateWorkflowTemplate;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Domain.Entities.WorkflowTemplates.Conditions;
using Cindi.Domain.Entities.WorkflowTemplates.ValueObjects;
using Cindi.Domain.Exceptions.Global;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Exceptions.StepTemplates;
using Cindi.Test.Global;
using Cindi.Test.Global.TestData;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Cindi.Test.Global.TestData.FibonacciSampleData;
using DefaultValue = Cindi.Domain.Entities.WorkflowTemplates.ValueObjects.DefaultValue;

namespace Cindi.Application.Tests.WorkflowTemplates.Commands
{
    public class CreateWorkflowTemplateCommandHandler_tests
    {
        [Fact]
        public async void DetectMissingStepTemplates()
        {
            FibonacciWorkflowData data = new FibonacciWorkflowData(5);
            Mock<IWorkflowTemplatesRepository> workflowTemplatesRepository = new Mock<IWorkflowTemplatesRepository>();
            workflowTemplatesRepository.Setup(sr => sr.GetWorkflowTemplateAsync(data.workflowTemplateWithInputs.Id)).Returns(Task.FromResult(data.workflowTemplateWithInputs));
            Mock<IStepTemplatesRepository> stepsRepository = new Mock<IStepTemplatesRepository>();

            var node = Utility.GetMockConsensusCoreNode();

            var mockStateLogger = new Mock<ILogger<CreateWorkflowTemplateCommandHandler>>();

            var handler = new CreateWorkflowTemplateCommandHandler(workflowTemplatesRepository.Object, stepsRepository.Object, node.Object, mockStateLogger.Object);

            await Assert.ThrowsAsync<StepTemplateNotFoundException>(async () => await handler.Handle(new CreateWorkflowTemplateCommand()
            {
                Name = data.workflowTemplateWithInputs.Name,
                Version = data.workflowTemplateWithInputs.Version,
                InputDefinitions = data.workflowTemplateWithInputs.InputDefinitions,
                LogicBlocks = data.workflowTemplateWithInputs.LogicBlocks
            }, new System.Threading.CancellationToken()));
        }

        [Fact]
        public async void DetectMissingWorkflowStepMapping()
        {
            FibonacciWorkflowData data = new FibonacciWorkflowData(5);
            Mock<IWorkflowTemplatesRepository> workflowTemplatesRepository = new Mock<IWorkflowTemplatesRepository>();
            workflowTemplatesRepository.Setup(sr => sr.GetWorkflowTemplateAsync(data.workflowTemplateWithInputs.Id)).Returns(Task.FromResult(data.workflowTemplateWithInputs));
            Mock<IStepTemplatesRepository> stepsRepository = new Mock<IStepTemplatesRepository>();
            stepsRepository.Setup(sr => sr.GetStepTemplateAsync(It.IsAny<string>())).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));

            var node = Utility.GetMockConsensusCoreNode();

            var mockStateLogger = new Mock<ILogger<CreateWorkflowTemplateCommandHandler>>();

            var handler = new CreateWorkflowTemplateCommandHandler(workflowTemplatesRepository.Object, stepsRepository.Object, node.Object, mockStateLogger.Object);

            await Assert.ThrowsAsync<MissingStepException>(async () =>
            await handler.Handle(new CreateWorkflowTemplateCommand()
            {
                Name = data.workflowTemplateWithInputs.Name,
                Version = data.workflowTemplateWithInputs.Version,
                InputDefinitions = data.workflowTemplateWithInputs.InputDefinitions,
                LogicBlocks = new List<LogicBlock>()
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
                    Prerequisites = new ConditionGroup
                    {
                        Operator = "AND",
                        Conditions = new List<Condition>(){ new StepStatusCondition()
                        {
                    StepRefId = 0,
                    Status = StepStatuses.Successful,
                    StatusCode = 0
                        }},
                    },
                    SubsequentSteps = new List<SubsequentStep> {
                         new SubsequentStep(){
                             StepTemplateId =StepTemplate.ReferenceId,
                             StepRefId = 1,
                                      Mappings = new List<Mapping>(){
                                      new Mapping()
                                       {
                                          OutputReferences = new StepOutputReference[]
                                          {
                                              new StepOutputReference()
                                              {
                                                  StepRefId = 3,
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
            }
            }, new System.Threading.CancellationToken()));
        }

        [Fact]
        public async void DetectMissingWorkflowStepOutputMapping()
        {
            FibonacciWorkflowData data = new FibonacciWorkflowData(5);
            Mock<IWorkflowTemplatesRepository> workflowTemplatesRepository = new Mock<IWorkflowTemplatesRepository>();
            workflowTemplatesRepository.Setup(sr => sr.GetWorkflowTemplateAsync(data.workflowTemplateWithInputs.Id)).Returns(Task.FromResult(data.workflowTemplateWithInputs));
            Mock<IStepTemplatesRepository> stepsRepository = new Mock<IStepTemplatesRepository>();
            stepsRepository.Setup(sr => sr.GetStepTemplateAsync(It.IsAny<string>())).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));

            var node = Utility.GetMockConsensusCoreNode();

            var mockStateLogger = new Mock<ILogger<CreateWorkflowTemplateCommandHandler>>();

            var handler = new CreateWorkflowTemplateCommandHandler(workflowTemplatesRepository.Object, stepsRepository.Object, node.Object, mockStateLogger.Object);

            await Assert.ThrowsAsync<MissingOutputException>(async () =>
            await handler.Handle(new CreateWorkflowTemplateCommand()
            {
                Name = data.workflowTemplateWithInputs.Name,
                Version = data.workflowTemplateWithInputs.Version,
                InputDefinitions = data.workflowTemplateWithInputs.InputDefinitions,
                LogicBlocks = new List<LogicBlock>()
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
                    Prerequisites = new ConditionGroup
                    {
                        Operator = "AND",
                        Conditions = new List<Condition>(){ new StepStatusCondition()
                        {
                    StepRefId = 0,
                    Status = StepStatuses.Successful,
                    StatusCode = 0
                        }},
                    },
                    SubsequentSteps = new List<SubsequentStep> {
                         new SubsequentStep(){
                             StepTemplateId =StepTemplate.ReferenceId,
                             StepRefId = 1,
                                      Mappings = new List<Mapping>(){
                                      new Mapping()
                                       {
                                          OutputReferences = new StepOutputReference[]
                                          {
                                              new StepOutputReference()
                                              {
                                                  StepRefId = 0,
                                                  OutputId = "z"
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
            }
            }, new System.Threading.CancellationToken()));
        }

        [Fact]
        public async void DetectMissingStartingWorkflowStep()
        {

            FibonacciWorkflowData data = new FibonacciWorkflowData(5);
            Mock<IWorkflowTemplatesRepository> workflowTemplatesRepository = new Mock<IWorkflowTemplatesRepository>();
            workflowTemplatesRepository.Setup(sr => sr.GetWorkflowTemplateAsync(data.workflowTemplateWithInputs.Id)).Returns(Task.FromResult(data.workflowTemplateWithInputs));
            Mock<IStepTemplatesRepository> stepsRepository = new Mock<IStepTemplatesRepository>();

            var node = Utility.GetMockConsensusCoreNode();

            var mockStateLogger = new Mock<ILogger<CreateWorkflowTemplateCommandHandler>>();

            var handler = new CreateWorkflowTemplateCommandHandler(workflowTemplatesRepository.Object, stepsRepository.Object, node.Object, mockStateLogger.Object);

            await Assert.ThrowsAsync<NoValidStartingLogicBlockException>(async () => await handler.Handle(new CreateWorkflowTemplateCommand()
            {
                Name = data.workflowTemplateWithInputs.Name,
                Version = data.workflowTemplateWithInputs.Version,
                InputDefinitions = data.workflowTemplateWithInputs.InputDefinitions,
                LogicBlocks = new List<LogicBlock>()
                {
                }
            }, new System.Threading.CancellationToken()));
        }


    }
}
