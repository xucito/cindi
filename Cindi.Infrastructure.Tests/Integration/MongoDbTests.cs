using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Sequences;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Enums;
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
            
            var steps = await stepsRepository.GetStepsAsync(1, 0);
            Assert.NotEmpty(steps);
            Assert.Null(steps[0].CompletedOn);
            Assert.Empty(steps[0].Outputs);
            Assert.Equal(StepStatuses.Unassigned, steps[0].Status);
            Assert.False(steps[0].IsComplete());
            Assert.Equal("testUser@email.com", steps[0].CreatedBy);
        }

        [Fact]
        public async void ChangeStepAssignment()
        {
            await stepTemplatesRepository.InsertAsync(FibonacciSampleData.StepTemplate);
            var createdStepId = await stepsRepository.InsertStepAsync(FibonacciSampleData.Step);

            var step = await stepsRepository.GetStepAsync(createdStepId);

            Assert.NotNull(step);
            Assert.Equal(StepStatuses.Unassigned, step.Status);
            Assert.False(step.IsComplete());
            Assert.Equal("testUser@email.com", step.CreatedBy);
            step.UpdateJournal(new Domain.Entities.JournalEntries.JournalEntry()
            {
                CreatedOn = DateTime.UtcNow,
                CreatedBy = SystemUsers.QUEUE_MANAGER,
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
            Assert.True(await stepsRepository.UpdateStep(step));

            Assert.NotEmpty(step.Journal.JournalEntries);
            Assert.Equal(StepStatuses.Assigned, step.Status);
            Assert.False(step.IsComplete());
            step.UpdateJournal(new Domain.Entities.JournalEntries.JournalEntry()
            {
                CreatedOn = DateTime.UtcNow,
                CreatedBy = SystemUsers.QUEUE_MANAGER,
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
            Assert.True(await stepsRepository.UpdateStep(step));
            step = await stepsRepository.GetStepAsync(step.Id);
            Assert.Equal(StepStatuses.Successful, step.Status);
            Assert.True(step.IsComplete());

            var steps = await stepsRepository.GetStepsAsync();

            Assert.Equal(StepStatuses.Successful, steps[0].Status);
            Assert.True(steps[0].IsComplete());

        }

        [Fact]
        public async void DetectStepQueues()
        {
            await stepTemplatesRepository.InsertAsync(FibonacciSampleData.StepTemplate);
            var createdStepId = await stepsRepository.InsertStepAsync(FibonacciSampleData.Step);



            var step = await stepsRepository.GetStepAsync(createdStepId);

            Assert.NotNull(step);
            Assert.Equal(StepStatuses.Unassigned, step.Status);
            Assert.False(step.IsComplete());
            Assert.NotNull(await stepsRepository.GetStepsAsync(1,0,StepStatuses.Unassigned));
            step.UpdateJournal(new Domain.Entities.JournalEntries.JournalEntry()
            {
                CreatedOn = DateTime.UtcNow,
                CreatedBy = SystemUsers.QUEUE_MANAGER,
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
            Assert.True(await stepsRepository.UpdateStep(step));
            Assert.Empty(await stepsRepository.GetStepsAsync(1, 0, StepStatuses.Unassigned));
            Assert.NotEmpty(await stepsRepository.GetStepsAsync(1, 0, StepStatuses.Assigned));
            step.UpdateJournal(new Domain.Entities.JournalEntries.JournalEntry()
            {
                CreatedOn = DateTime.UtcNow,
                CreatedBy = SystemUsers.QUEUE_MANAGER,
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
            Assert.True(await stepsRepository.UpdateStep(step));
            Assert.Empty(await stepsRepository.GetStepsAsync(1, 0, StepStatuses.Unassigned));
            Assert.Empty(await stepsRepository.GetStepsAsync(1, 0, StepStatuses.Assigned));
            Assert.Empty(await stepsRepository.GetStepsAsync(1, 0, StepStatuses.Warning));
            Assert.Empty(await stepsRepository.GetStepsAsync(1, 0, StepStatuses.Error));
            Assert.NotEmpty(await stepsRepository.GetStepsAsync(1, 0, StepStatuses.Successful));
        }

        [Fact]
        public async void CompleteStep()
        {
            await stepTemplatesRepository.InsertAsync(FibonacciSampleData.StepTemplate);
            var createdStepId = await stepsRepository.InsertStepAsync(FibonacciSampleData.Step);
            var step = await stepsRepository.GetStepAsync(createdStepId);
            var status = step.Status;

            Assert.NotNull(step);
            Assert.Equal(StepStatuses.Unassigned, step.Status);
            step.UpdateJournal(new Domain.Entities.JournalEntries.JournalEntry()
            {
                CreatedOn = DateTime.UtcNow,
                CreatedBy = SystemUsers.QUEUE_MANAGER,
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
            Assert.True(await stepsRepository.UpdateStep(step));
            step = await stepsRepository.GetStepAsync(createdStepId);
            Assert.Empty(await stepsRepository.GetStepsAsync(1, 0, StepStatuses.Unassigned));
            Assert.NotEmpty(await stepsRepository.GetStepsAsync(1, 0, StepStatuses.Assigned));
            step.UpdateJournal(new Domain.Entities.JournalEntries.JournalEntry()
            {
                CreatedOn = DateTime.UtcNow,
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
                        Value = "TEST",
                    }
                }
            });
            Assert.True(await stepsRepository.UpdateStep(step));

            Assert.NotEmpty(await stepsRepository.GetStepsAsync(1, 0, StepStatuses.Successful));
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

            var id = Guid.NewGuid();

            var newSequence = new Domain.Entities.Sequences.Sequence(
                id,
                data.sequenceTemplate.Id,
                new Dictionary<string, object>(),
                "",
                "",
                DateTime.UtcNow)
            {
            };

            var newSequenceId = await sequenceRepository.InsertSequenceAsync(newSequence);

            await sequenceRepository.UpsertSequenceMetadataAsync(id);

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

        [Fact]
        public async void GetSequenceMetadata()
        {
            FibonacciSequenceData data = new FibonacciSequenceData(5);
            await sequenceTemplatesRepository.InsertSequenceTemplateAsync(data.sequenceTemplate);

            var id = Guid.NewGuid();
            var newSequence = new Domain.Entities.Sequences.Sequence(
                id,
                data.sequenceTemplate.Id,
                new Dictionary<string, object>(),
                "",
                "",
                DateTime.UtcNow)
            {
            };

            var newSequenceId = await sequenceRepository.InsertSequenceAsync(newSequence);

            Assert.NotNull(newSequence.Metadata);
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
