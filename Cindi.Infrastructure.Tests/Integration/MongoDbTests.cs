using Cindi.Infrastructure.Tests.TestData;
using Cindi.Persistence;
using Cindi.Persistence.Steps;
using Cindi.Persistence.StepTemplates;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cindi.Infrastructure.Tests.Integration
{
    public class MongoDbTests: IAsyncLifetime
    {
        public MongoDbTests()
        {
            BaseRepository.RegisterClassMaps();
        }

        [Fact]
        public async void CreateStep()
        {

            var stepTemplatesRepository = new StepTemplatesRepository(GlobalTestSettings.CindiDBConnectionString, GlobalTestSettings.TestDBName);
            await stepTemplatesRepository.InsertAsync(FibonacciSampleData.StepTemplate);

            var stepsRepository = new StepsRepository(GlobalTestSettings.CindiDBConnectionString, GlobalTestSettings.TestDBName);
            await stepsRepository.InsertStepAsync(FibonacciSampleData.Step);

            var steps = await stepsRepository.GetStepsAsync(0,1000);

            Assert.NotEmpty(steps);
        }

        public Task DisposeAsync()
        {
            var client = new MongoClient(GlobalTestSettings.CindiDBConnectionString);
            client.DropDatabase(GlobalTestSettings.TestDBName);
            return Task.CompletedTask;
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
