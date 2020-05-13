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
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Node;
using ConsensusCore.Node.Communication.Controllers;
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

        Mock<IClusterStateService> clusterMoq = new Mock<IClusterStateService>();

        Mock<IClusterRequestHandler> _node;
        Mock<IOptionsMonitor<CindiClusterOptions>> _optionsMonitor;


        public CompleteStepCommandHandler_tests()
        {

            ClusterStateService.GetEncryptionKey = () =>
            {
                return "GCSPHNKWXHPNELFEACOFIWGGUCVWZLUY";
            };
            _node = Utility.GetMockConsensusCoreNode();


            _optionsMonitor = new Mock<IOptionsMonitor<CindiClusterOptions>>();
            _optionsMonitor.Setup(o => o.CurrentValue).Returns(cindiClusterOptions);
        }

        static CindiClusterOptions cindiClusterOptions = new CindiClusterOptions()
        {
            DefaultSuspensionTimeMs = 0
        };

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

            Mock<IEntitiesRepository> entitiesRepository = new Mock<IEntitiesRepository>();
            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync<Step>(It.IsAny<Expression<Func<Step,bool>>>())).Returns(Task.FromResult(TestStep));
           entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync<StepTemplate>(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync<WorkflowTemplate>(It.IsAny<Expression<Func<WorkflowTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.WorkflowTemplate));
            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync<Workflow>(It.IsAny<Expression<Func<Workflow, bool>>>())).Returns(Task.FromResult(TestWorkflow));
            entitiesRepository.Setup(sr => sr.GetAsync<Step>(It.IsAny<Expression<Func<Step, bool>>>(), null, null, 10, 0)).Returns(Task.FromResult((IEnumerable<Step>)new List<Step>() { TestStep }));


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

            
            entitiesRepository.Setup(kr => kr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<BotKey, bool>>>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            Mock<IClusterStateService> service = new Mock<IClusterStateService>();
            service.Setup(m => m.IsLogicBlockLocked(It.IsAny<Guid>(), It.IsAny<string>())).Returns(false);
            service.Setup(m => m.LockLogicBlock(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(1));
            service.Setup(m => m.WasLockObtained(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(true);

            var handler = new CompleteStepCommandHandler(entitiesRepository.Object, service.Object, mockLogger.Object, _optionsMonitor.Object, mediator.Object, _node.Object);

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
            Mock<IEntitiesRepository> entitiesRepository = new Mock<IEntitiesRepository>();
            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync<StepTemplate>(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync<Step>(It.IsAny<Expression<Func<Step, bool>>>())).Returns(Task.FromResult(TestStep));
            _node.Setup(n => n.Handle(It.IsAny<AddShardWriteOperation>())).Returns(Task.FromResult(new AddShardWriteOperationResponse() { IsSuccessful = true }));

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

            
            entitiesRepository.Setup(kr => kr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<BotKey, bool>>>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            Mock<IClusterStateService> service = new Mock<IClusterStateService>();

            var handler = new CompleteStepCommandHandler(entitiesRepository.Object, service.Object, mockLogger.Object, _optionsMonitor.Object, mediator.Object, _node.Object);

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
            Mock<IEntitiesRepository> entitiesRepository = new Mock<IEntitiesRepository>();
            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync<StepTemplate>(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(stepTemplate));
            _node.Setup(n => n.Handle(It.IsAny<AddShardWriteOperation>())).Returns(Task.FromResult(new AddShardWriteOperationResponse() { IsSuccessful = true }));
            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync<Step>(It.IsAny<Expression<Func<Step,bool>>>())).Returns(Task.FromResult(TestStep));

            var mockLogger = new Mock<ILogger<CompleteStepCommandHandler>>();
            Mock<IMediator> mediator = new Mock<IMediator>();

            var mockStateLogger = new Mock<ILogger<ClusterStateService>>();
            var testKey = SecurityUtility.GenerateRSAKeyPair();

            
            entitiesRepository.Setup(kr => kr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<BotKey, bool>>>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var node = Utility.GetMockConsensusCoreNode();
            Mock<IClusterStateService> service = new Mock<IClusterStateService>();

            var handler = new CompleteStepCommandHandler(entitiesRepository.Object, service.Object, mockLogger.Object, _optionsMonitor.Object, mediator.Object, _node.Object);

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
            
            
            
           entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync<StepTemplate>(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep));
            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync<Step>(It.IsAny<Expression<Func<Step,bool>>>())).Returns(Task.FromResult(TestStep));

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

            
            entitiesRepository.Setup(kr => kr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<BotKey, bool>>>())).Returns(Task.FromResult(new BotKey()
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
