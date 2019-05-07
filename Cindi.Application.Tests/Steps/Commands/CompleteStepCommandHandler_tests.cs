﻿using Cindi.Application.Interfaces;
using Cindi.Application.Options;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Application.Steps.Commands.CompleteStep;
using Cindi.Application.Steps.Commands.CreateStep;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.Sequences;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using Cindi.Test.Global.TestData;
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
        }

        [Fact]
        public async void CompleteStepWithSequence()
        {
            var TestSequence = FibonacciSampleData.Sequence;
            TestSequence.Journal = new Domain.Entities.JournalEntries.Journal(
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
                        Value = TestSequence.Id
                    },
                    new Update()
                    {
                        FieldName = "steprefid",
                        Type = UpdateType.Create,
                        Value = 0
                    },
                }
            });

            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep.Id));
            stepsRepository.Setup(s => s.GetStepAsync(TestStep.Id)).Returns(Task.FromResult(TestStep));

            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(FibonacciSampleData.StepTemplate.Id)).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));

            Mock<ISequenceTemplatesRepository> sequenceTemplateRepository = new Mock<ISequenceTemplatesRepository>();
            sequenceTemplateRepository.Setup(str => str.GetSequenceTemplateAsync(TestSequence.SequenceTemplateId)).Returns(Task.FromResult(FibonacciSampleData.SequenceTemplate));

            Mock<ISequencesRepository> sequenceRepository = new Mock<ISequencesRepository>();
            sequenceRepository.Setup(sr => sr.GetSequenceAsync(TestSequence.Id)).Returns(Task.FromResult(TestSequence));
            sequenceRepository.Setup(sr => sr.GetSequenceStepsAsync(TestSequence.Id)).Returns(Task.FromResult(new List<Step>() { TestStep }));


            var mockLogger = new Mock<ILogger<CompleteStepCommandHandler>>();
            Mock<IClusterRepository> clusterRepository = new Mock<IClusterRepository>();
            var mockStateLogger = new Mock<ILogger<ClusterStateService>>();

            Mock<IMediator> mediator = new Mock<IMediator>();

            var secondStepId = Guid.NewGuid();
            mediator.Setup(m => m.Send(It.IsAny<CreateStepCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new CommandResult<Step>()
                {
                    ObjectRefId = secondStepId.ToString(),
                    Result = new Step("",
                "",
                FibonacciSampleData.StepTemplate.Id,
                "admin",
                secondStepId,
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

            var handler = new CompleteStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, sequenceTemplateRepository.Object, sequenceRepository.Object, new ClusterStateService(clusterRepository.Object, mockStateLogger.Object, serviceScopeFactory.Object), mockLogger.Object, cindiClusterOptions, mediator.Object, keysRepository.Object);

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
        public async void CompleteStepWithNoSequence()
        {
            var TestStep = FibonacciSampleData.Step;
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            Mock<ISequenceTemplatesRepository> sequenceTemplateRepository = new Mock<ISequenceTemplatesRepository>();
            Mock<ISequencesRepository> sequenceRepository = new Mock<ISequencesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(FibonacciSampleData.StepTemplate.Id)).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep.Id));
            stepsRepository.Setup(s => s.GetStepAsync(TestStep.Id)).Returns(Task.FromResult(TestStep));
            stepsRepository.Setup(s => s.UpdateStep(TestStep)).Returns(Task.FromResult(true));

            var mockLogger = new Mock<ILogger<CompleteStepCommandHandler>>();
            Mock<IMediator> mediator = new Mock<IMediator>();

            var secondStepId = Guid.NewGuid();
            mediator.Setup(m => m.Send(It.IsAny<CreateStepCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new CommandResult<Step>()
                {
                    ObjectRefId = secondStepId.ToString(),
                    Result = new Step("",
                "",
                FibonacciSampleData.StepTemplate.Id,
                "admin",
                secondStepId,
                new Dictionary<string, object>()
                {
                    {"n-1",2 },
                    {"n-2",1 }
                }
                )
                }));

            Mock<IClusterRepository> clusterRepository = new Mock<IClusterRepository>();
            var mockStateLogger = new Mock<ILogger<ClusterStateService>>();
            var testKey = SecurityUtility.GenerateRSAKeyPair();

            Mock<IBotKeysRepository> keysRepository = new Mock<IBotKeysRepository>();
            keysRepository.Setup(kr => kr.GetBotKeyAsync(It.IsAny<Guid>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var handler = new CompleteStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, sequenceTemplateRepository.Object, sequenceRepository.Object, new ClusterStateService(clusterRepository.Object, mockStateLogger.Object, serviceScopeFactory.Object), mockLogger.Object, cindiClusterOptions, mediator.Object, keysRepository.Object);

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
            var TestStep = SecretSampleData.StepTemplate.GenerateStep(stepTemplate.Id, "", "", "", new Dictionary<string, object>() {
                {"secret", testPhrase}
            }, null, null, null, ClusterStateService.GetEncryptionKey());
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            Mock<ISequenceTemplatesRepository> sequenceTemplateRepository = new Mock<ISequenceTemplatesRepository>();
            Mock<ISequencesRepository> sequenceRepository = new Mock<ISequencesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(stepTemplate.Id)).Returns(Task.FromResult(stepTemplate));
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep.Id));
            stepsRepository.Setup(s => s.GetStepAsync(TestStep.Id)).Returns(Task.FromResult(TestStep));
            stepsRepository.Setup(s => s.UpdateStep(TestStep)).Returns(Task.FromResult(true));

            var mockLogger = new Mock<ILogger<CompleteStepCommandHandler>>();
            Mock<IMediator> mediator = new Mock<IMediator>();

            Mock<IClusterRepository> clusterRepository = new Mock<IClusterRepository>();
            var mockStateLogger = new Mock<ILogger<ClusterStateService>>();
            var testKey = SecurityUtility.GenerateRSAKeyPair();

            Mock<IBotKeysRepository> keysRepository = new Mock<IBotKeysRepository>();
            keysRepository.Setup(kr => kr.GetBotKeyAsync(It.IsAny<Guid>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var handler = new CompleteStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, sequenceTemplateRepository.Object, sequenceRepository.Object, new ClusterStateService(clusterRepository.Object, mockStateLogger.Object, serviceScopeFactory.Object), mockLogger.Object, cindiClusterOptions, mediator.Object, keysRepository.Object);

            var completeResult = await handler.Handle(new CompleteStepCommand()
            {
                Id = TestStep.Id,
                Outputs = new Dictionary<string, object>()
                {
                    { "secret", SecurityUtility.RsaEncryptWithPublic(testPhrase, testKey.PublicKey) }
                },
                Status = StepStatuses.Successful,
                StatusCode = 0,
                Log = "TEST"
            }, new System.Threading.CancellationToken());

            Assert.Equal(TestStep.Id.ToString(), completeResult.ObjectRefId);
        }

        [Fact]
        public async void DetectMissingSequenceOnCompleteStep()
        {
            var TestSequence = FibonacciSampleData.Sequence;
            var TestStep = FibonacciSampleData.Step;

            TestStep.UpdateJournal(new JournalEntry()
            {
                Updates = new List<Update>()
                {
                    new Update()
                    {
                        FieldName = "sequenceid",
                        Type = UpdateType.Create,
                        Value = TestSequence.Id
                    }
                }
            });

            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            Mock<ISequenceTemplatesRepository> sequenceTemplateRepository = new Mock<ISequenceTemplatesRepository>();
            Mock<ISequencesRepository> sequenceRepository = new Mock<ISequencesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(FibonacciSampleData.StepTemplate.Id)).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep.Id));
            stepsRepository.Setup(s => s.GetStepAsync(TestStep.Id)).Returns(Task.FromResult(TestStep));

            var mockLogger = new Mock<ILogger<CompleteStepCommandHandler>>();

            Mock<IMediator> mediator = new Mock<IMediator>();

            var secondStepId = Guid.NewGuid();
            mediator.Setup(m => m.Send(It.IsAny<CreateStepCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new CommandResult<Step>()
                {
                    ObjectRefId = secondStepId.ToString(),
                    Result = new Step("",
                "",
                FibonacciSampleData.StepTemplate.Id,
                "admin",
                secondStepId,
                new Dictionary<string, object>()
                {
                    {"n-1",2 },
                    {"n-2",1 }
                }
                )
                }));

            Mock<IClusterRepository> clusterRepository = new Mock<IClusterRepository>();
            var mockStateLogger = new Mock<ILogger<ClusterStateService>>();
            var testKey = SecurityUtility.GenerateRSAKeyPair();

            Mock<IBotKeysRepository> keysRepository = new Mock<IBotKeysRepository>();
            keysRepository.Setup(kr => kr.GetBotKeyAsync(It.IsAny<Guid>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var handler = new CompleteStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, sequenceTemplateRepository.Object, sequenceRepository.Object, new ClusterStateService(clusterRepository.Object, mockStateLogger.Object, serviceScopeFactory.Object), mockLogger.Object, cindiClusterOptions, mediator.Object, keysRepository.Object);

            await Assert.ThrowsAsync<MissingSequenceException>(async () => await handler.Handle(new CompleteStepCommand()
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
        }
    }
}
