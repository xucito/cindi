using Cindi.Application.Interfaces;
using Cindi.Application.Options;
using Cindi.Application.Services.ClusterState;
using Cindi.Application.Steps.Commands;
using Cindi.Application.Steps.Commands.AssignStep;
using Cindi.Application.Steps.Commands.CompleteStep;
using Cindi.Application.Steps.Commands.CreateStep;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.Sequences;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Exceptions.StepTemplates;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using Cindi.Test.Global.MockInterfaces;
using Cindi.Test.Global.TestData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cindi.Application.Tests.Steps.Commands
{
    public class CreateStepCommandHandler_Tests
    {

        Mock<IServiceScopeFactory> serviceScopeFactory = new Mock<IServiceScopeFactory>();

        Mock<IClusterStateService> clusterMoq = new Mock<IClusterStateService>();

        static CindiClusterOptions cindiClusterOptions = new CindiClusterOptions()
        {
            DefaultSuspensionTime = 0
        };

        public CreateStepCommandHandler_Tests()
        {

            ClusterStateService.GetEncryptionKey = () =>
            {
                return "GCSPHNKWXHPNELFEACOFIWGGUCVWZLUY";
            };
        }

        [Fact]
        public async void DetectMissingTemplate()
        {
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();

            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, clusterMoq.Object);

            await Assert.ThrowsAsync<StepTemplateNotFoundException>(async () =>
            {
                await handler.Handle(new CreateStepCommand()
                {
                    StepTemplateId = FibonacciSampleData.StepTemplate.Id

                }, new System.Threading.CancellationToken());
            });
        }

        [Fact]
        public async void DetectCorrectTemplate()
        {
            var TestStep = FibonacciSampleData.Step;
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(FibonacciSampleData.StepTemplate.Id)).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep));

            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, clusterMoq.Object);

            var commandResult = await handler.Handle(new CreateStepCommand()
            {
                StepTemplateId = FibonacciSampleData.StepTemplate.Id,
                Inputs = new Dictionary<string, object>()
                {
                    { "n-1", 1 } ,
                    { "n-2", 2 }
                }
            }, new System.Threading.CancellationToken());

            Assert.NotNull(handler);
            Assert.Equal(TestStep.Id.ToString(), commandResult.ObjectRefId);
        }

        [Fact]
        public async void DetectNoInputs()
        {
            var TestStep = FibonacciSampleData.Step;
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(FibonacciSampleData.StepTemplate.Id)).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep));

            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, clusterMoq.Object);

            await Assert.ThrowsAsync<InvalidStepInputException>(async () => await handler.Handle(new CreateStepCommand()
            {
                StepTemplateId = FibonacciSampleData.StepTemplate.Id
            }, new System.Threading.CancellationToken()));
        }

        [Fact]
        public async void DetectTooManyInputs()
        {
            var TestStep = FibonacciSampleData.Step;
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(FibonacciSampleData.StepTemplate.Id)).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep));

            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, clusterMoq.Object);

            await Assert.ThrowsAsync<InvalidStepInputException>(async () => await handler.Handle(new CreateStepCommand()
            {
                StepTemplateId = FibonacciSampleData.StepTemplate.Id,
                Inputs = new Dictionary<string, object>()
                {
                    { "n-1", 1 } ,
                    { "n-2", 2 },
                    { "n-3", 2 }
                }
            }, new System.Threading.CancellationToken()));
        }

        [Fact]
        public async void DetectMissingInputs()
        {
            var TestStep = FibonacciSampleData.Step;
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(FibonacciSampleData.StepTemplate.Id)).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep));
            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, clusterMoq.Object);

            await Assert.ThrowsAsync<InvalidStepInputException>(async () => await handler.Handle(new CreateStepCommand()
            {
                StepTemplateId = FibonacciSampleData.StepTemplate.Id,
                Inputs = new Dictionary<string, object>()
                {
                    { "n-1", 1 }
                }
            }, new System.Threading.CancellationToken()));
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
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep));
            stepsRepository.Setup(s => s.GetStepAsync(TestStep.Id)).Returns(Task.FromResult(TestStep));


            var mockLogger = new Mock<ILogger<CompleteStepCommandHandler>>();

            Mock<IClusterRepository> clusterRepository = new Mock<IClusterRepository>();
            var mockStateLogger = new Mock<ILogger<ClusterStateService>>();
            var handler = new CompleteStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, sequenceTemplateRepository.Object, sequenceRepository.Object, new ClusterStateService(clusterRepository.Object, mockStateLogger.Object, serviceScopeFactory.Object), mockLogger.Object, cindiClusterOptions);

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
        public async void DetectMissingSequenceOnCompleteStep()
        {
            var TestSequence = FibonacciSampleData.Sequence;
            var TestStep = FibonacciSampleData.Step;
            TestStep.SequenceId = TestSequence.Id;

            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            Mock<ISequenceTemplatesRepository> sequenceTemplateRepository = new Mock<ISequenceTemplatesRepository>();
            Mock<ISequencesRepository> sequenceRepository = new Mock<ISequencesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(FibonacciSampleData.StepTemplate.Id)).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep));
            stepsRepository.Setup(s => s.GetStepAsync(TestStep.Id)).Returns(Task.FromResult(TestStep));

            var mockLogger = new Mock<ILogger<CompleteStepCommandHandler>>();

            Mock<IClusterRepository> clusterRepository = new Mock<IClusterRepository>();
            var mockStateLogger = new Mock<ILogger<ClusterStateService>>();
            var handler = new CompleteStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, sequenceTemplateRepository.Object, sequenceRepository.Object, new ClusterStateService(clusterRepository.Object, mockStateLogger.Object, serviceScopeFactory.Object), mockLogger.Object, cindiClusterOptions);

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

        [Fact]
        public async void CreateStepWithSecret()
        {
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(SecretSampleData.StepTemplate.Id)).Returns(Task.FromResult(SecretSampleData.StepTemplate));

            var stepTemplate = await stepTemplatesRepository.Object.GetStepTemplateAsync(SecretSampleData.StepTemplate.Id);
            var newStep = stepTemplate.GenerateStep(stepTemplate.Id, "", "", "", new Dictionary<string, object>() {
                {"secret", "This is a test"}
            });

            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            stepsRepository.Setup(st => st.InsertStepAsync(Moq.It.IsAny<Step>())).Returns(Task.FromResult(newStep));

            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, clusterMoq.Object);

            var step = await handler.Handle(new CreateStepCommand()
            {
                StepTemplateId = stepTemplate.Id,
                Inputs = new Dictionary<string, object>()
                {
                    {"secret", "This is a test" }
                }
            }, new System.Threading.CancellationToken());

            //Test encryption of step worked
            Assert.NotEqual("This is a test", (string)step.Result.Inputs["secret"]);
            //Test decryption of step
            Assert.Equal("This is a test", SecurityUtility.SymmetricallyDecrypt((string)step.Result.Inputs["secret"], ClusterStateService.GetEncryptionKey()));
        }

        [Fact]
        public async void AssignStepWithSecret()
        {
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(SecretSampleData.StepTemplate.Id)).Returns(Task.FromResult(SecretSampleData.StepTemplate));
            var testPhrase = "This is a test";
            var stepTemplate = await stepTemplatesRepository.Object.GetStepTemplateAsync(SecretSampleData.StepTemplate.Id);
            var newStep = stepTemplate.GenerateStep(stepTemplate.Id, "", "", "", new Dictionary<string, object>() {
                {"secret", testPhrase}
            });

            newStep.EncryptStepSecrets(Domain.Enums.EncryptionProtocol.AES256, stepTemplate, ClusterStateService.GetEncryptionKey());

            clusterMoq.Setup(cm => cm.IsAssignmentEnabled()).Returns(true);

            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            stepsRepository.Setup(st => st.GetStepsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string[]>())).Returns(Task.FromResult(new List<Step> { newStep }));

            var testKey = SecurityUtility.GenerateRSAKeyPair();

            Mock<IBotKeysRepository> keysRepository = new Mock<IBotKeysRepository>();
            keysRepository.Setup(kr => kr.GetBotKeyAsync(It.IsAny<Guid>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var handler = new AssignStepCommandHandler(stepsRepository.Object, clusterMoq.Object, stepTemplatesRepository.Object, keysRepository.Object);

            var result = await handler.Handle(new AssignStepCommand()
            {
            }, new System.Threading.CancellationToken());

            var assignedStep = result.Result;

            Assert.NotEqual(testPhrase, (string)result.Result.Inputs["secret"]);

            var encryptionTestResult = SecurityUtility.RsaEncryptWithPublic(testPhrase, testKey.PublicKey);
            //Randomized padding is used
            Assert.NotEqual(encryptionTestResult, (string)result.Result.Inputs["secret"]);
            //Decryption using private key should work
            Assert.Equal(SecurityUtility.RsaDecryptWithPrivate(encryptionTestResult, testKey.PrivateKey), SecurityUtility.RsaDecryptWithPrivate((string)result.Result.Inputs["secret"], testKey.PrivateKey));
        }

        [Fact]
        public async void CompleteStepWithSequence()
        {
            var TestSequence = FibonacciSampleData.Sequence;
            TestSequence.Journal = new Domain.Entities.JournalEntries.Journal(new List<Domain.Entities.JournalEntries.JournalEntry>() {
                new Domain.Entities.JournalEntries.JournalEntry()
                {
                        SubjectId = TestSequence.Id,
                        ChainId = 0,
                        Entity = JournalEntityTypes.Sequence,
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
            });

            var TestStep = FibonacciSampleData.Step;
            TestStep.SequenceId = TestSequence.Id;
            TestStep.StepRefId = 0;


            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep));
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
            var handler = new CompleteStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, sequenceTemplateRepository.Object, sequenceRepository.Object, new ClusterStateService(clusterRepository.Object, mockStateLogger.Object, serviceScopeFactory.Object), mockLogger.Object, cindiClusterOptions);

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
    }
}
