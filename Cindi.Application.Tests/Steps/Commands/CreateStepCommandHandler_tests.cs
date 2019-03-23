﻿using Cindi.Application.Interfaces;
using Cindi.Application.Options;
using Cindi.Application.Services.ClusterState;
using Cindi.Application.Steps.Commands;
using Cindi.Application.Steps.Commands.CompleteStep;
using Cindi.Application.Steps.Commands.CreateStep;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.Sequences;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Exceptions.StepTemplates;
using Cindi.Domain.ValueObjects;
using Cindi.Test.Global.MockInterfaces;
using Cindi.Test.Global.TestData;
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

        static CindiClusterOptions cindiClusterOptions = new CindiClusterOptions()
        {
            DefaultSuspensionTime = 0
        };

        [Fact]
        public async void DetectMissingTemplate()
        {
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();

            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object);

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

            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object);

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

            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object);

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

            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object);

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
            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object);

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
            var handler = new CompleteStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, sequenceTemplateRepository.Object, sequenceRepository.Object, new ClusterStateService(clusterRepository.Object, mockStateLogger.Object), mockLogger.Object, cindiClusterOptions);

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
            var handler = new CompleteStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, sequenceTemplateRepository.Object, sequenceRepository.Object, new ClusterStateService(clusterRepository.Object, mockStateLogger.Object), mockLogger.Object, cindiClusterOptions);

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
            var handler = new CompleteStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, sequenceTemplateRepository.Object, sequenceRepository.Object, new ClusterStateService(clusterRepository.Object, mockStateLogger.Object), mockLogger.Object, cindiClusterOptions);

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
