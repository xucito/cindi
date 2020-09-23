using Cindi.Application.Interfaces;
using Cindi.Application.Services.ClusterOperation;
using Cindi.Application.Workflows.Commands;
using Cindi.Application.WorkflowTemplates.Commands.CreateWorkflowTemplate;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Cindi.Test.Global.TestData.FibonacciSampleData;
using DefaultValue = Cindi.Domain.Entities.WorkflowTemplates.ValueObjects.DefaultValue;

namespace Cindi.Application.Tests.WorkflowTemplates.Commands
{
    public class CreateWorkflowTemplateCommandHandler_tests
    {
        Mock<IClusterService> clusterService = new Mock<IClusterService>();

        [Fact]
        public async void DetectMissingStepTemplates()
        {
            FibonacciWorkflowData data = new FibonacciWorkflowData(5);
            
           // clusterService.Setup(sr => sr.GetFirstOrDefaultAsync<WorkflowTemplate>(It.IsAny<Expression<Func<WorkflowTemplate, bool>>>())).Returns(Task.FromResult(data.workflowTemplateWithInputs));
            
            var node = Utility.GetMockConsensusCoreNode();

            var mockStateLogger = new Mock<ILogger<CreateWorkflowTemplateCommandHandler>>();

            var handler = new CreateWorkflowTemplateCommandHandler(mockStateLogger.Object, clusterService.Object);

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
            
           // clusterService.Setup(sr => sr.GetFirstOrDefaultAsync<WorkflowTemplate>(It.IsAny<Expression<Func<WorkflowTemplate, bool>>>())).Returns(Task.FromResult(data.workflowTemplateWithInputs));

           clusterService.Setup(sr => sr.GetFirstOrDefaultAsync<StepTemplate>(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));

            var node = Utility.GetMockConsensusCoreNode();

            var mockStateLogger = new Mock<ILogger<CreateWorkflowTemplateCommandHandler>>();

            var handler = new CreateWorkflowTemplateCommandHandler(mockStateLogger.Object, clusterService.Object);

            await Assert.ThrowsAsync<MissingStepException>(async () =>
            await handler.Handle(new CreateWorkflowTemplateCommand()
            {
                Name = data.workflowTemplateWithInputs.Name,
                Version = data.workflowTemplateWithInputs.Version,
                InputDefinitions = data.workflowTemplateWithInputs.InputDefinitions,
                LogicBlocks = new Dictionary<string, LogicBlock>()
            {
                    { "0", new LogicBlock()
                {
                    Dependencies = new ConditionGroup
                    {
                        Operator = "OR",
                        Conditions = new Dictionary<string, Condition>(){},
                    },
                    SubsequentSteps = new Dictionary<string, SubsequentStep>{
                        {"0",
                         new SubsequentStep(){
                             StepTemplateId = FibonacciSampleData.StepTemplate.ReferenceId,
                                      Mappings = new Dictionary<string, Mapping>(){
                                          {"n-1",
                                      new Mapping()
                                       {
                                           DefaultValue = new DefaultValue(){
                                               Value = 1
                                           },
                                       } },
                                      { "n-2",
                                      new Mapping(){
                                           DefaultValue = new DefaultValue(){
                                               Value = 1
                                           }
                                       }
                                   }
                                      }
                         } }
                } } },
                    { "1",
                new LogicBlock()
                {
                    Dependencies = new ConditionGroup
                    {
                        Operator = "AND",
                        Conditions = new Dictionary<string, Condition>{
                            {"0", new StepStatusCondition()
                        {
                    StepName = "0",
                    Status = StepStatuses.Successful,
                    StatusCode = 0
                        }} }
                    },
                    SubsequentSteps = new Dictionary<string, SubsequentStep> {
                        { "1",
                         new SubsequentStep(){
                             StepTemplateId =FibonacciSampleData.StepTemplate.ReferenceId,
                                      Mappings = new Dictionary<string, Mapping>(){
                                          {"n-1",
                                      new Mapping()
                                       {
                                          OutputReferences = new StepOutputReference[]
                                          {
                                              new StepOutputReference()
                                              {
                                                  StepName = "3",
                                                  OutputId = "n"
                                              }
                                          },
                                           DefaultValue = new DefaultValue(){
                                               Value = 1
                                           }
                                       } },
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
            }, new System.Threading.CancellationToken()));
        }

        [Fact]
        public async void DetectMissingWorkflowStepOutputMapping()
        {
            FibonacciWorkflowData data = new FibonacciWorkflowData(5);
            //clusterService.Setup(sr => sr.GetFirstOrDefaultAsync<WorkflowTemplate>(It.IsAny<Expression<Func<WorkflowTemplate, bool>>>())).Returns(Task.FromResult(data.workflowTemplateWithInputs));
           clusterService.Setup(sr => sr.GetFirstOrDefaultAsync<StepTemplate>(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            var node = Utility.GetMockConsensusCoreNode();

            var mockStateLogger = new Mock<ILogger<CreateWorkflowTemplateCommandHandler>>();

            var handler = new CreateWorkflowTemplateCommandHandler(mockStateLogger.Object, clusterService.Object);

            await Assert.ThrowsAsync<MissingOutputException>(async () =>
            await handler.Handle(new CreateWorkflowTemplateCommand()
            {
                Name = data.workflowTemplateWithInputs.Name,
                Version = data.workflowTemplateWithInputs.Version,
                InputDefinitions = data.workflowTemplateWithInputs.InputDefinitions,
                LogicBlocks = new Dictionary<string, LogicBlock>()
            {
                    { "0",
                new LogicBlock()
                {
                    Dependencies = new ConditionGroup
                    {
                        Operator = "OR",
                        Conditions = new Dictionary<string, Condition>(){},
                    },
                    SubsequentSteps = new Dictionary<string, SubsequentStep> {
                        { "0",
                         new SubsequentStep(){
                             StepTemplateId =FibonacciSampleData.StepTemplate.ReferenceId,
                                      Mappings = new Dictionary<string, Mapping>(){
                                          {"n-1",
                                      new Mapping()
                                       {
                                           DefaultValue = new DefaultValue(){
                                               Value = 1
                                           }
                                       } },
                                      {
                                              "n-2",
                                      new Mapping(){
                                           DefaultValue = new DefaultValue(){
                                               Value = 1
                                           }
                                      }
                                   }
                                      }
                         } }
                }
                    } },
                { "1",
                new LogicBlock()
                {
                    Dependencies = new ConditionGroup
                    {
                        Operator = "AND",
                        Conditions = new Dictionary<string, Condition>(){
                            {"0", new StepStatusCondition()
                        {
                    StepName = "0",
                    Status = StepStatuses.Successful,
                    StatusCode = 0
                        }} }
                    },
                    SubsequentSteps = new Dictionary<string, SubsequentStep>{
                        { "1",
                         new SubsequentStep(){
                             StepTemplateId =FibonacciSampleData.StepTemplate.ReferenceId,
                                      Mappings = new Dictionary<string, Mapping>(){
                                          {"n-1",
                                      new Mapping()
                                       {
                                          OutputReferences = new StepOutputReference[]
                                          {
                                              new StepOutputReference()
                                              {
                                                  StepName = "0",
                                                  OutputId = "z"
                                              }
                                          },
                                           DefaultValue = new DefaultValue(){
                                               Value = 1
                                           }
                                       } },
                                      {
                                              "n-2",
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
            }, new System.Threading.CancellationToken()));
        }

        [Fact]
        public async void DetectMissingStartingWorkflowStep()
        {

            FibonacciWorkflowData data = new FibonacciWorkflowData(5);
            
            //clusterService.Setup(sr => sr.GetFirstOrDefaultAsync<WorkflowTemplate>(It.IsAny<Expression<Func<WorkflowTemplate, bool>>>())).Returns(Task.FromResult(data.workflowTemplateWithInputs));
            

            var node = Utility.GetMockConsensusCoreNode();

            var mockStateLogger = new Mock<ILogger<CreateWorkflowTemplateCommandHandler>>();

            var handler = new CreateWorkflowTemplateCommandHandler(mockStateLogger.Object, clusterService.Object);

            await Assert.ThrowsAsync<NoValidStartingLogicBlockException>(async () => await handler.Handle(new CreateWorkflowTemplateCommand()
            {
                Name = data.workflowTemplateWithInputs.Name,
                Version = data.workflowTemplateWithInputs.Version,
                InputDefinitions = data.workflowTemplateWithInputs.InputDefinitions,
                LogicBlocks = new Dictionary<string, LogicBlock>()
                {
                }
            }, new System.Threading.CancellationToken()));
        }


    }
}
