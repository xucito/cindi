using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Sequences;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.ValueObjects;
using Cindi.Persistence;
using Cindi.Persistence.Sequences;
using Cindi.Persistence.SequenceTemplates;
using Cindi.Persistence.Steps;
using Cindi.Persistence.StepTemplates;
using Cindi.Test.Global.TestData;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Cindi.Test.Global.TestData.FibonacciSampleData;

namespace Cindi.Infrastructure.Tests.Integration
{
    public class MongoDb_Tests : IAsyncLifetime, IClassFixture<MongoDBFixture>
    {
        public string TestDBId;
        public StepTemplatesRepository stepTemplatesRepository;
        public StepsRepository stepsRepository;
        public SequenceTemplatesRepository sequenceTemplatesRepository;
        public SequencesRepository sequenceRepository;

        public MongoDb_Tests(MongoDBFixture fixture)
        {

        }

        [Fact]
        public async void CreateStep()
        {
            await stepTemplatesRepository.InsertAsync(FibonacciSampleData.StepTemplate);
            var createdStep = await stepsRepository.InsertStepAsync(FibonacciSampleData.Step);

            await stepsRepository.InsertJournalEntryAsync(new JournalEntry()
            {
                SubjectId = createdStep.Id,
                ChainId = 0,
                Entity = JournalEntityTypes.Step,
                RecordedOn = DateTime.UtcNow,
                Updates = new List<Update>()
                {
                    new Update()
                    {
                        FieldName = "status",
                        Value = StepStatuses.Unassigned,
                        Type = UpdateType.Override
                    }
                }
            });

            var steps = await stepsRepository.GetStepsAsync(0, 1000);

            Assert.NotEmpty(steps);
            Assert.Null(steps[0].CompletedOn);
            Assert.Empty(steps[0].TestResults);
            Assert.Empty(steps[0].Outputs);
            Assert.Equal(StepStatuses.Unassigned, steps[0].Status);
            Assert.False(steps[0].IsComplete);
        }

        [Fact]
        public async void ChangeStepAssignment()
        {
            await stepTemplatesRepository.InsertAsync(FibonacciSampleData.StepTemplate);
            var createdStep = await stepsRepository.InsertStepAsync(FibonacciSampleData.Step);
            await stepsRepository.InsertJournalEntryAsync(new JournalEntry()
            {
                SubjectId = createdStep.Id,
                ChainId = 0,
                Entity = JournalEntityTypes.Step,
                RecordedOn = DateTime.UtcNow,
                Updates = new List<Update>()
                {
                    new Update()
                    {
                        FieldName = "status",
                        Value = StepStatuses.Unassigned,
                        Type = UpdateType.Override
                    }
                }
            });

            var step = await stepsRepository.GetStepAsync(createdStep.Id);

            Assert.NotNull(step);
            Assert.Equal(StepStatuses.Unassigned, step.Status);
            Assert.False(step.IsComplete);

            await stepsRepository.InsertJournalEntryAsync(new Domain.Entities.JournalEntries.JournalEntry()
            {
                Entity = JournalEntityTypes.Step,
                SubjectId = step.Id,
                RecordedOn = DateTime.UtcNow,
                ChainId = 1,
                Updates = new List<Domain.ValueObjects.Update>()
                {
                    new Update()
                    {
                        Type = UpdateType.Override,
                        FieldName = "status",
                        Value = StepStatuses.Assigned,
                    }

                }
            });

            step = await stepsRepository.GetStepAsync(createdStep.Id);
            Assert.NotEmpty(step.Journal.Entries[0].Id);
            Assert.Equal(StepStatuses.Assigned, step.Status);
            Assert.False(step.IsComplete);

            await stepsRepository.InsertJournalEntryAsync(new Domain.Entities.JournalEntries.JournalEntry()
            {
                Entity = JournalEntityTypes.Step,
                SubjectId = step.Id,
                RecordedOn = DateTime.UtcNow,
                ChainId = 2,
                Updates = new List<Domain.ValueObjects.Update>()
                {
                    new Update()
                    {
                        Type = UpdateType.Override,
                        FieldName = "status",
                        Value = StepStatuses.Successful
                    }

                }
            });

            step = await stepsRepository.GetStepAsync(createdStep.Id);
            Assert.Equal(StepStatuses.Successful, step.Status);
            Assert.True(step.IsComplete);

            var steps = await stepsRepository.GetStepsAsync();

            Assert.Equal(StepStatuses.Successful, steps[0].Status);
            Assert.True(steps[0].IsComplete);

        }

        [Fact]
        public async void DetectStepQueues()
        {
            await stepTemplatesRepository.InsertAsync(FibonacciSampleData.StepTemplate);
            var createdStep = await stepsRepository.InsertStepAsync(FibonacciSampleData.Step);
            await stepsRepository.InsertJournalEntryAsync(new JournalEntry()
            {
                SubjectId = createdStep.Id,
                ChainId = 0,
                Entity = JournalEntityTypes.Step,
                RecordedOn = DateTime.UtcNow,
                Updates = new List<Update>()
                {
                    new Update()
                    {
                        FieldName = "status",
                        Value = StepStatuses.Unassigned,
                        Type = UpdateType.Override
                    }
                }
            });



            var step = await stepsRepository.GetStepAsync(createdStep.Id);
            var clusterStateService = new ClusterStateService();
            var state = clusterStateService.GetLastStepAssignmentCheckpoints(new string[] { FibonacciSampleData.StepTemplate.Id });

            Assert.NotNull(step);
            Assert.Equal(StepStatuses.Unassigned, step.Status);
            Assert.False(step.IsComplete);
            Assert.NotNull(await stepsRepository.GetStepsAsync(StepStatuses.Unassigned, state));

            await stepsRepository.InsertJournalEntryAsync(new Domain.Entities.JournalEntries.JournalEntry()
            {
                Entity = JournalEntityTypes.Step,
                SubjectId = step.Id,
                RecordedOn = DateTime.UtcNow,
                ChainId = 1,
                Updates = new List<Domain.ValueObjects.Update>()
                {
                    new Update()
                    {
                        Type = UpdateType.Override,
                        FieldName = "status",
                        Value = StepStatuses.Assigned,
                    }

                }
            });

            Assert.Null(await stepsRepository.GetStepsAsync(StepStatuses.Unassigned, state));
            Assert.NotNull(await stepsRepository.GetStepsAsync(StepStatuses.Assigned, state));

            await stepsRepository.InsertJournalEntryAsync(new Domain.Entities.JournalEntries.JournalEntry()
            {
                Entity = JournalEntityTypes.Step,
                SubjectId = step.Id,
                RecordedOn = DateTime.UtcNow,
                ChainId = 2,
                Updates = new List<Domain.ValueObjects.Update>()
                {
                    new Update()
                    {
                        Type = UpdateType.Override,
                        FieldName = "status",
                        Value = StepStatuses.Successful,
                    }
                }
            });

            Assert.Null(await stepsRepository.GetStepsAsync(StepStatuses.Unassigned, state));
            Assert.Null(await stepsRepository.GetStepsAsync(StepStatuses.Assigned, state));
            Assert.Null(await stepsRepository.GetStepsAsync(StepStatuses.Warning, state));
            Assert.Null(await stepsRepository.GetStepsAsync(StepStatuses.Error, state));
            Assert.NotNull(await stepsRepository.GetStepsAsync(StepStatuses.Successful, state));
        }

        [Fact]
        public async void CompleteStep()
        {
            await stepTemplatesRepository.InsertAsync(FibonacciSampleData.StepTemplate);
            var createdStep = await stepsRepository.InsertStepAsync(FibonacciSampleData.Step);
            await stepsRepository.InsertJournalEntryAsync(new JournalEntry()
            {
                SubjectId = createdStep.Id,
                ChainId = 0,
                Entity = JournalEntityTypes.Step,
                RecordedOn = DateTime.UtcNow,
                Updates = new List<Update>()
                {
                    new Update()
                    {
                        FieldName = "status",
                        Value = StepStatuses.Unassigned,
                        Type = UpdateType.Override
                    }
                }
            });



            var step = await stepsRepository.GetStepAsync(createdStep.Id);
            var clusterStateService = new ClusterStateService();
            var state = clusterStateService.GetLastStepAssignmentCheckpoints(new string[] { FibonacciSampleData.StepTemplate.Id });

            Assert.NotNull(step);
            Assert.Equal(StepStatuses.Unassigned, step.Status);

            await stepsRepository.InsertJournalEntryAsync(new Domain.Entities.JournalEntries.JournalEntry()
            {
                Entity = JournalEntityTypes.Step,
                SubjectId = step.Id,
                RecordedOn = DateTime.UtcNow,
                ChainId = 1,
                Updates = new List<Domain.ValueObjects.Update>()
                {
                    new Update()
                    {
                        Type = UpdateType.Override,
                        FieldName = "status",
                        Value = StepStatuses.Assigned,
                    }

                }
            });

            Assert.Null(await stepsRepository.GetStepsAsync(StepStatuses.Unassigned, state));
            Assert.NotNull(await stepsRepository.GetStepsAsync(StepStatuses.Assigned, state));

            await stepsRepository.InsertJournalEntryAsync(new Domain.Entities.JournalEntries.JournalEntry()
            {
                Entity = JournalEntityTypes.Step,
                SubjectId = step.Id,
                RecordedOn = DateTime.UtcNow,
                ChainId = 2,
                Updates = new List<Domain.ValueObjects.Update>()
                {
                    new Update()
                    {
                        Type = UpdateType.Override,
                        FieldName = "status",
                        Value = StepStatuses.Successful,
                    },
                    new Update()
                    {
                        Type = UpdateType.Override,
                        FieldName = "outputs",
                        Value = new Dictionary<string, object>(){{"n-1", 3 }, {"n-2", 4} },
                    },
                    new Update()
                    {
                        Type = UpdateType.Override,
                        FieldName = "statuscode",
                        Value = 1,
                    },
                    new Update()
                    {
                        Type = UpdateType.Append,
                        FieldName = "logs",
                        Value = new string[]{ "This is a test." },
                    }
                }
            });

            Assert.NotNull(await stepsRepository.GetStepsAsync(StepStatuses.Successful, state));
            var completedStep = await stepsRepository.GetStepAsync(step.Id);
            Assert.NotEmpty(completedStep.Logs);
            Assert.NotEmpty(completedStep.Outputs);
            Assert.Equal(1, completedStep.StatusCode);
            Assert.Equal(StepStatuses.Successful, completedStep.Status);
        }

        [Fact]
        public async void CreateSequence()
        {
            FibonacciSequenceData data = new FibonacciSequenceData(5);
            await sequenceTemplatesRepository.InsertSequenceTemplateAsync(data.sequenceTemplate);

            var newSequence =  await sequenceRepository.InsertSequenceAsync(new Domain.Entities.Sequences.Sequence()
            {
                SequenceTemplateId = data.sequenceTemplate.Id,
                Inputs = new Dictionary<string, object>(),
                CreatedOn = DateTime.UtcNow,
                Id = Guid.NewGuid()
            });

            await sequenceRepository.InsertJournalEntryAsync(new JournalEntry()
            {
                SubjectId = newSequence.Id,
                ChainId = 0,
                Entity = JournalEntityTypes.Sequence,
                RecordedOn = DateTime.UtcNow,
                Updates = new List<Update>()
                {
                    new Update()
                    {
                        FieldName = "status",
                        Value = SequenceStatuses.Started,
                        Type = UpdateType.Override
                    }
                }
            });

            Assert.NotNull(await sequenceRepository.GetSequenceAsync(newSequence.Id));
            Assert.Single((await sequenceRepository.GetSequencesAsync()));
            Assert.Equal(SequenceStatuses.Started, (await sequenceRepository.GetSequenceAsync(newSequence.Id)).Status);
            Assert.Equal(SequenceStatuses.Started, (await sequenceRepository.GetSequencesAsync())[0].Status);
        }

        [Fact]
        public async void CreateSequenceTemplate()
        {
            FibonacciSequenceData data = new FibonacciSequenceData(5);
            await sequenceTemplatesRepository.InsertSequenceTemplateAsync(data.sequenceTemplateWithInputs);

            Assert.NotNull(await sequenceTemplatesRepository.GetSequenceTemplateAsync(data.sequenceTemplate.Id));
            Assert.NotEmpty(await sequenceTemplatesRepository.GetSequenceTemplatesAsync());
        }

        public Task DisposeAsync()
        {
            var client = new MongoClient(GlobalTestSettings.CindiDBConnectionString);
            client.DropDatabase(TestDBId);
            return Task.CompletedTask;
        }

        public Task InitializeAsync()
        {
            TestDBId = GlobalTestSettings.TestDBName + this.GetType().Name + Guid.NewGuid();
            //BaseRepository.RegisterClassMaps();
            stepTemplatesRepository = new StepTemplatesRepository(GlobalTestSettings.CindiDBConnectionString, TestDBId);
            stepsRepository = new StepsRepository(GlobalTestSettings.CindiDBConnectionString, TestDBId);
            sequenceTemplatesRepository = new SequenceTemplatesRepository(GlobalTestSettings.CindiDBConnectionString, TestDBId);
            sequenceRepository = new SequencesRepository(GlobalTestSettings.CindiDBConnectionString, TestDBId);
            return Task.CompletedTask;
        }
    }
}
