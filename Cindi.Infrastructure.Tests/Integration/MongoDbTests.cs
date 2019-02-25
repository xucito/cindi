using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.ValueObjects;
using Cindi.Persistence;
using Cindi.Persistence.Steps;
using Cindi.Persistence.StepTemplates;
using Cindi.Test.Global.TestData;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cindi.Infrastructure.Tests.Integration
{
    public class MongoDb_Tests : IAsyncLifetime, IClassFixture<MongoDBFixture>
    {
        public string TestDBId;
        public StepTemplatesRepository stepTemplatesRepository;
        public StepsRepository stepsRepository;

        public MongoDb_Tests(MongoDBFixture fixture)
        {

        }

        [Fact]
        public async void CreateStep()
        {
            await stepTemplatesRepository.InsertAsync(FibonacciSampleData.StepTemplate);
            await stepsRepository.InsertStepAsync(FibonacciSampleData.Step);
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
            var step = await stepsRepository.GetStepAsync(createdStep.Id);

            Assert.NotNull(step);
            Assert.Equal(StepStatuses.Unassigned, step.Status);
            Assert.False(step.IsComplete);
            Assert.NotNull(await stepsRepository.GetStepsAsync(StepStatuses.Unassigned, new string[] { createdStep.StepTemplateId }));

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

            Assert.Null(await stepsRepository.GetStepsAsync(StepStatuses.Unassigned, new string[] { createdStep.StepTemplateId }));
            Assert.NotNull(await stepsRepository.GetStepsAsync(StepStatuses.Assigned, new string[] { createdStep.StepTemplateId }));

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

            Assert.Null(await stepsRepository.GetStepsAsync(StepStatuses.Unassigned, new string[] { createdStep.StepTemplateId }));
            Assert.Null(await stepsRepository.GetStepsAsync(StepStatuses.Assigned, new string[] { createdStep.StepTemplateId }));
            Assert.Null(await stepsRepository.GetStepsAsync(StepStatuses.Warning, new string[] { createdStep.StepTemplateId }));
            Assert.Null(await stepsRepository.GetStepsAsync(StepStatuses.Error, new string[] { createdStep.StepTemplateId }));
            Assert.NotNull(await stepsRepository.GetStepsAsync(StepStatuses.Successful, new string[] { createdStep.StepTemplateId }));
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
            return Task.CompletedTask;
        }
    }
}
