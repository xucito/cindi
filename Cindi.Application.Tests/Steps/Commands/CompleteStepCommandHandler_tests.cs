using Cindi.Application.Interfaces;
using Cindi.Application.Options;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Application.Steps.Commands.CompleteStep;
using Cindi.Application.Steps.Commands.CreateStep;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.Workflows;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using Cindi.Test.Global;
using Cindi.Test.Global.TestData;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Node;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cindi.Application.Tests.Steps.Commands
{
    public class CompleteStepCommandHandler_tests
    {
        Mock<IServiceScopeFactory> serviceScopeFactory = new Mock<IServiceScopeFactory>();

        Mock<IClusterStateService> clusterMoq = new Mock<IClusterStateService>();

        Mock<IConsensusCoreNode<CindiClusterState, IBaseRepository>> _node;

        static CindiClusterOptions cindiClusterOptions = new CindiClusterOptions()
        {
            DefaultSuspensionTime = 0
        };

        public CompleteStepCommandHandler_tests()
        {

            ClusterStateService.GetEncryptionKey = () =>
            {
                return "GCSPHNKWXHPNELFEACOFIWGGUCVWZLUY";
            };
            _node = Utility.GetMockConsensusCoreNode();

        }

        [Fact]
        public async void CompleteStepWithWorkflow()
        {
            var TestWorkflow = FibonacciSampleData.Workflow;
            TestWorkflow.Journal = new Domain.Entities.JournalEntries.Journal(
                new Domain.Entities.JournalEntries.JournalEntry()
                {
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "testuser@email.com",
                    Updates = new List<Update>()
                        {
                            new Update()
                            {
                                FieldName = "status",
                                Value = StepStatuses.Unassigned,
                                Type = UpdateType.Override
                            }
                        }
                }
           );

            var TestStep = FibonacciSampleData.Step;
            TestStep.UpdateJournal(new JournalEntry()
            {
                Updates = new List<Update>()
                {
                    new Update()
                    {
                        FieldName = "sequenceid",
                        Type = UpdateType.Create,
                        Value = TestWorkflow.Id
                    },
                    new Update()
                    {
                        FieldName = "workflowstepid",
                        Type = UpdateType.Create,
                        Value = 0
                    },
                }
            });

            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep));
            stepsRepository.Setup(s => s.GetStepAsync(TestStep.Id)).Returns(Task.FromResult(TestStep));

            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(FibonacciSampleData.StepTemplate.ReferenceId)).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));

            Mock<IWorkflowTemplatesRepository> workflowTemplateRepository = new Mock<IWorkflowTemplatesRepository>();
            workflowTemplateRepository.Setup(str => str.GetWorkflowTemplateAsync(TestWorkflow.WorkflowTemplateId)).Returns(Task.FromResult(FibonacciSampleData.WorkflowTemplate));

            Mock<IWorkflowsRepository> sequenceRepository = new Mock<IWorkflowsRepository>();
            sequenceRepository.Setup(sr => sr.GetWorkflowAsync(TestWorkflow.Id)).Returns(Task.FromResult(TestWorkflow));
            sequenceRepository.Setup(sr => sr.GetWorkflowStepsAsync(TestWorkflow.Id)).Returns(Task.FromResult(new List<Step>() { TestStep }));


            var mockLogger = new Mock<ILogger<CompleteStepCommandHandler>>();
            var mockStateLogger = new Mock<ILogger<ClusterStateService>>();

            Mock<IMediator> mediator = new Mock<IMediator>();

            var secondStepId = Guid.NewGuid();
            mediator.Setup(m => m.Send(It.IsAny<CreateStepCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new CommandResult<Step>()
                {
                    ObjectRefId = secondStepId.ToString(),
                    Result = new Step(secondStepId
                    , "",
                "",
                FibonacciSampleData.StepTemplate.ReferenceId,
                "admin",
                new Dictionary<string, object>()
                {
                    {"n-1",2 },
                    {"n-2",1 }
                }
                )
                }));
            var testKey = SecurityUtility.GenerateRSAKeyPair();

            Mock<IBotKeysRepository> keysRepository = new Mock<IBotKeysRepository>();
            keysRepository.Setup(kr => kr.GetBotKeyAsync(It.IsAny<Guid>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            Mock<IClusterStateService> service = new Mock<IClusterStateService>();
            service.Setup(m => m.IsLogicBlockLocked(It.IsAny<Guid>(), It.IsAny<string>())).Returns(false);
            service.Setup(m => m.LockLogicBlock(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(1));
            _node.Setup(n => n.HasEntryBeenCommitted(It.IsAny<int>())).Returns(true);
            service.Setup(m => m.WasLockObtained(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(true);

            var handler = new CompleteStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, workflowTemplateRepository.Object, sequenceRepository.Object, service.Object, mockLogger.Object, cindiClusterOptions, mediator.Object, keysRepository.Object, _node.Object);

            Assert.Equal(TestStep.Id.ToString(), (await handler.Handle(new CompleteStepCommand()
            {
                Id = TestStep.Id,
                Outputs = new Dictionary<string, object>()
                {
                    { "n", 0 }
                },
                Status = StepStatuses.Successful,
                StatusCode = 0,
                Log = "TEST"
            }, new System.Threading.CancellationToken())).ObjectRefId);

        }


        [Fact]
        public async void CompleteStepWithNoWorkflow()
        {
            var TestStep = FibonacciSampleData.Step;
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            Mock<IWorkflowTemplatesRepository> workflowTemplateRepository = new Mock<IWorkflowTemplatesRepository>();
            Mock<IWorkflowsRepository> sequenceRepository = new Mock<IWorkflowsRepository>();

            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(FibonacciSampleData.StepTemplate.ReferenceId)).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));

            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep));
            stepsRepository.Setup(s => s.GetStepAsync(TestStep.Id)).Returns(Task.FromResult(TestStep));

            stepsRepository.Setup(s => s.UpdateStep(TestStep)).Returns(Task.FromResult(TestStep));

            var mockLogger = new Mock<ILogger<CompleteStepCommandHandler>>();
            Mock<IMediator> mediator = new Mock<IMediator>();

            var secondStepId = Guid.NewGuid();
            mediator.Setup(m => m.Send(It.IsAny<CreateStepCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new CommandResult<Step>()
                {
                    ObjectRefId = secondStepId.ToString(),
                    Result = new Step(secondStepId,
                    "",
                "",
                FibonacciSampleData.StepTemplate.ReferenceId,
                "admin",
                new Dictionary<string, object>()
                {
                    {"n-1",2 },
                    {"n-2",1 }
                }
                )
                }));

            var mockStateLogger = new Mock<ILogger<ClusterStateService>>();
            var testKey = SecurityUtility.GenerateRSAKeyPair();

            Mock<IBotKeysRepository> keysRepository = new Mock<IBotKeysRepository>();
            keysRepository.Setup(kr => kr.GetBotKeyAsync(It.IsAny<Guid>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var handler = new CompleteStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, workflowTemplateRepository.Object, sequenceRepository.Object, new ClusterStateService(mockStateLogger.Object, serviceScopeFactory.Object, _node.Object), mockLogger.Object, cindiClusterOptions, mediator.Object, keysRepository.Object, _node.Object);

            var completeResult = await handler.Handle(new CompleteStepCommand()
            {
                Id = TestStep.Id,
                Outputs = new Dictionary<string, object>()
                {
                    { "n", 0 }
                },
                Status = StepStatuses.Successful,
                StatusCode = 0,
                Log = "TEST"
            }, new System.Threading.CancellationToken());

            Assert.Equal(TestStep.Id.ToString(), completeResult.ObjectRefId);
        }

        [Fact]
        public async void EncryptOutputOnCompletion()
        {
            var stepTemplate = SecretSampleData.StepTemplate;
            var testPhrase = "This is a test";
            var TestStep = SecretSampleData.StepTemplate.GenerateStep(stepTemplate.ReferenceId, "", "", "", new Dictionary<string, object>() {
                {"secret", testPhrase}
            }, null, null, ClusterStateService.GetEncryptionKey());
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            Mock<IWorkflowTemplatesRepository> workflowTemplateRepository = new Mock<IWorkflowTemplatesRepository>();
            Mock<IWorkflowsRepository> sequenceRepository = new Mock<IWorkflowsRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(stepTemplate.ReferenceId)).Returns(Task.FromResult(stepTemplate));
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep));
            stepsRepository.Setup(s => s.GetStepAsync(TestStep.Id)).Returns(Task.FromResult(TestStep));
            stepsRepository.Setup(s => s.UpdateStep(TestStep)).Returns(Task.FromResult(TestStep));

            var mockLogger = new Mock<ILogger<CompleteStepCommandHandler>>();
            Mock<IMediator> mediator = new Mock<IMediator>();

            var mockStateLogger = new Mock<ILogger<ClusterStateService>>();
            var testKey = SecurityUtility.GenerateRSAKeyPair();

            Mock<IBotKeysRepository> keysRepository = new Mock<IBotKeysRepository>();
            keysRepository.Setup(kr => kr.GetBotKeyAsync(It.IsAny<Guid>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var node = Utility.GetMockConsensusCoreNode();

            var handler = new CompleteStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, workflowTemplateRepository.Object, sequenceRepository.Object, new ClusterStateService(mockStateLogger.Object, serviceScopeFactory.Object, node.Object), mockLogger.Object, cindiClusterOptions, mediator.Object, keysRepository.Object, _node.Object);

            var completeResult = await handler.Handle(new CompleteStepCommand()
            {
                Id = TestStep.Id,
                Outputs = new Dictionary<string, object>()
                {
                    { "secret", SecurityUtility.RsaEncryptWithPrivate(testPhrase, testKey.PrivateKey) }
                },
                Status = StepStatuses.Successful,
                StatusCode = 0,
                Log = "TEST"
            }, new System.Threading.CancellationToken());

            Assert.Equal(TestStep.Id.ToString(), completeResult.ObjectRefId);
        }

        /*[Fact]
        public async void DetectMissingWorkflowOnCompleteStep()
        {
            var TestWorkflow = FibonacciSampleData.Workflow;
            var TestStep = FibonacciSampleData.Step;

            TestStep.UpdateJournal(new JournalEntry()
            {
                Updates = new List<Update>()
                {
                    new Update()
                    {
                        FieldName = "sequenceid",
                        Type = UpdateType.Create,
                        Value = TestWorkflow.Id
                    }
                }
            });

            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            Mock<IWorkflowTemplatesRepository> workflowTemplateRepository = new Mock<IWorkflowTemplatesRepository>();
            Mock<IWorkflowsRepository> sequenceRepository = new Mock<IWorkflowsRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(FibonacciSampleData.StepTemplate.ReferenceId)).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep));
            stepsRepository.Setup(s => s.GetStepAsync(TestStep.Id)).Returns(Task.FromResult(TestStep));

            var mockLogger = new Mock<ILogger<CompleteStepCommandHandler>>();

            Mock<IMediator> mediator = new Mock<IMediator>();

            var secondStepId = Guid.NewGuid();
            mediator.Setup(m => m.Send(It.IsAny<CreateStepCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new CommandResult<Step>()
                {
                    ObjectRefId = secondStepId.ToString(),
                    Result = new Step(secondStepId,
                    "",
                "",
                FibonacciSampleData.StepTemplate.ReferenceId,
                "admin",
                new Dictionary<string, object>()
                {
                    {"n-1",2 },
                    {"n-2",1 }
                }
                )
                }));

            var mockStateLogger = new Mock<ILogger<ClusterStateService>>();
            var testKey = SecurityUtility.GenerateRSAKeyPair();

            Mock<IBotKeysRepository> keysRepository = new Mock<IBotKeysRepository>();
            keysRepository.Setup(kr => kr.GetBotKeyAsync(It.IsAny<Guid>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var handler = new CompleteStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, workflowTemplateRepository.Object, sequenceRepository.Object, new ClusterStateService(mockStateLogger.Object, serviceScopeFactory.Object, _node.Object), mockLogger.Object, cindiClusterOptions, mediator.Object, keysRepository.Object, _node.Object);

            await Assert.ThrowsAsync<MissingWorkflowException>(async () => await handler.Handle(new CompleteStepCommand()
            {
                Id = TestStep.Id,
                Outputs = new Dictionary<string, object>()
                {
                    { "n", 0 }
                },
                Status = StepStatuses.Successful,
                StatusCode = 0,
                Log = "TEST"
            }, new System.Threading.CancellationToken()));
        }*/
    }
}
