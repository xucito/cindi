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
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Domain.Exceptions.Workflows;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using Cindi.Test.Global;
using Cindi.Test.Global.TestData;




using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cindi.Application.Tests.Steps.Commands
{
    public class CompleteStepCommandHandler_tests
    {
        Mock<IServiceScopeFactory> serviceScopeFactory = new Mock<IServiceScopeFactory>();
        Mock<IOptionsMonitor<CindiClusterOptions>> _optionsMonitor;
        Mock<IEntitiesRepository> _entitiesRepositoryMock = new Mock<IEntitiesRepository>();
        Mock<IAssignmentCache> _assignmentCacheMock = new Mock<IAssignmentCache>();
        Mock<IStateMachine> _stateMachineMock = new Mock<IStateMachine>();

        static CindiClusterOptions cindiClusterOptions = new CindiClusterOptions()
        {
            DefaultSuspensionTimeMs = 0
        };

        public CompleteStepCommandHandler_tests()
        {
            _stateMachineMock.Setup(er => er.EncryptionKey).Returns("GCSPHNKWXHPNELFEACOFIWGGUCVWZLUY");

            _stateMachineMock.Setup(cm => cm.GetSettings).Returns(new ClusterSettings()
            {
            });

            _stateMachineMock.Setup(cm => cm.GetState()).Returns(new CindiClusterState()
            {
            });

            _optionsMonitor = new Mock<IOptionsMonitor<CindiClusterOptions>>();
            _optionsMonitor.Setup(o => o.CurrentValue).Returns(new CindiClusterOptions()
            {
                DefaultSuspensionTimeMs = 0
            });
        }

        [Fact]
        public async void CompleteStepWithWorkflow()
        {
            var TestWorkflow = FibonacciSampleData.Workflow;
            TestWorkflow.Status = StepStatuses.Unassigned;

            var TestStep = FibonacciSampleData.Step;
            TestStep.WorkflowId = TestWorkflow.Id;

            _entitiesRepositoryMock.Setup(sr => sr.GetFirstOrDefaultAsync<Step>(It.IsAny<Expression<Func<Step, bool>>>())).Returns(Task.FromResult(TestStep));
            _entitiesRepositoryMock.Setup(sr => sr.GetFirstOrDefaultAsync<StepTemplate>(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            _entitiesRepositoryMock.Setup(sr => sr.GetFirstOrDefaultAsync<WorkflowTemplate>(It.IsAny<Expression<Func<WorkflowTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.WorkflowTemplate));
            _entitiesRepositoryMock.Setup(sr => sr.GetFirstOrDefaultAsync<Workflow>(It.IsAny<Expression<Func<Workflow, bool>>>())).Returns(Task.FromResult(TestWorkflow));
            _entitiesRepositoryMock.Setup(sr => sr.GetAsync<Step>(It.IsAny<Expression<Func<Step, bool>>>(), null, null, 10, 0)).Returns(Task.FromResult((IEnumerable<Step>)new List<Step>() { TestStep }));


            var mockLogger = new Mock<ILogger<CompleteStepCommandHandler>>();

            Mock<IMediator> mediator = new Mock<IMediator>();

            var secondStepId = Guid.NewGuid();
            mediator.Setup(m => m.Send(It.IsAny<CreateStepCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new CommandResult<Step>()
                {
                    ObjectRefId = secondStepId.ToString(),
                    Result = new Step()
                    {
                        Id = secondStepId,
                        StepTemplateId = FibonacciSampleData.StepTemplate.ReferenceId,
                        Inputs = new Dictionary<string, object>()
                {
                    {"n-1",2 },
                    {"n-2",1 }
                }
                    }
                }));
            var testKey = SecurityUtility.GenerateRSAKeyPair();


            _entitiesRepositoryMock.Setup(kr => kr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<BotKey, bool>>>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            _stateMachineMock.Setup(m => m.IsLogicBlockLocked(It.IsAny<Guid>(), It.IsAny<string>())).Returns(false);
            _stateMachineMock.Setup(m => m.LockLogicBlock(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            _stateMachineMock.Setup(m => m.IsEntityLocked(It.IsAny<Guid>())).Returns(true);

            var handler = new CompleteStepCommandHandler(mockLogger.Object, _optionsMonitor.Object, _entitiesRepositoryMock.Object, _stateMachineMock.Object, mediator.Object);

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
        public async void DecryptStepOutputSecretForSubsequentStep()
        {
            Assert.True(false);
        }

        [Fact]
        public async void DecryptWorkflowInputSecretForSubsequentStep()
        {
            Assert.True(false);
        }


        [Fact]
        public async void CompleteStepWithNoWorkflow()
        {
            var TestStep = FibonacciSampleData.Step;
            _entitiesRepositoryMock.Setup(sr => sr.GetFirstOrDefaultAsync<StepTemplate>(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            _entitiesRepositoryMock.Setup(sr => sr.GetFirstOrDefaultAsync<Step>(It.IsAny<Expression<Func<Step, bool>>>())).Returns(Task.FromResult(TestStep));

            var mockLogger = new Mock<ILogger<CompleteStepCommandHandler>>();
            Mock<IMediator> mediator = new Mock<IMediator>();

            var secondStepId = Guid.NewGuid();
            mediator.Setup(m => m.Send(It.IsAny<CreateStepCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new CommandResult<Step>()
                {
                    ObjectRefId = secondStepId.ToString(),
                    Result = new Step()
                    {
                        Id = secondStepId,
                        StepTemplateId = FibonacciSampleData.StepTemplate.ReferenceId,
                        Inputs =
                new Dictionary<string, object>()
                {
                    {"n-1",2 },
                    {"n-2",1 }
                }
                    }
                }));

            var testKey = SecurityUtility.GenerateRSAKeyPair();


            _entitiesRepositoryMock.Setup(kr => kr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<BotKey, bool>>>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var handler = new CompleteStepCommandHandler(mockLogger.Object, _optionsMonitor.Object, _entitiesRepositoryMock.Object, _stateMachineMock.Object, mediator.Object);

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
            var _stateMachine = _stateMachineMock.Object;
            var stepTemplate = SecretSampleData.StepTemplate;
            var testPhrase = "This is a test";
            var TestStep = SecretSampleData.StepTemplate.GenerateStep(stepTemplate.ReferenceId, "", "", "", new Dictionary<string, object>() {
                {"secret", testPhrase}
            }, null, null, _stateMachine.EncryptionKey);
            _entitiesRepositoryMock.Setup(sr => sr.GetFirstOrDefaultAsync<StepTemplate>(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(stepTemplate));
            _entitiesRepositoryMock.Setup(sr => sr.GetFirstOrDefaultAsync<Step>(It.IsAny<Expression<Func<Step, bool>>>())).Returns(Task.FromResult(TestStep));

            var mockLogger = new Mock<ILogger<CompleteStepCommandHandler>>();
            Mock<IMediator> mediator = new Mock<IMediator>();
            var testKey = SecurityUtility.GenerateRSAKeyPair();


            _entitiesRepositoryMock.Setup(kr => kr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<BotKey, bool>>>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var handler = new CompleteStepCommandHandler(mockLogger.Object, _optionsMonitor.Object, _entitiesRepositoryMock.Object, _stateMachineMock.Object, mediator.Object);

            var generatedAESKey = SecurityUtility.RandomString(32);

            var completeResult = await handler.Handle(new CompleteStepCommand()
            {
                Id = TestStep.Id,
                Outputs = new Dictionary<string, object>()
                {
                    { "secret", SecurityUtility.SymmetricallyEncrypt(testPhrase, generatedAESKey) }
                },
                EncryptionKey = SecurityUtility.RsaEncryptWithPrivate(generatedAESKey, testKey.PrivateKey),
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
            
            
            
           _entitiesRepositoryMock.Setup(sr => sr.GetFirstOrDefaultAsync<StepTemplate>(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep));
            _entitiesRepositoryMock.Setup(sr => sr.GetFirstOrDefaultAsync<Step>(It.IsAny<Expression<Func<Step,bool>>>())).Returns(Task.FromResult(TestStep));

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

            
            _entitiesRepositoryMock.Setup(kr => kr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<BotKey, bool>>>())).Returns(Task.FromResult(new BotKey()
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
