using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cindi.Application.Interfaces;
using MongoDB.Bson.Serialization;

namespace Cindi.Persistence.Steps
{
    public class StepsRepository : BaseRepository, IStepsRepository
    {
        private IMongoCollection<Step> _steps;
        private IMongoCollection<JournalEntry> _journalEntries;

        public StepsRepository(string mongoDbConnectionString, string databaseName) : base(mongoDbConnectionString, databaseName)
        {
            var client = new MongoClient(mongoDbConnectionString);
            SetCollection(client);
        }

        public StepsRepository(IMongoClient client) : base(client)
        {
            SetCollection(client);
        }
        private void SetCollection(IMongoClient client)
        {
            var database = client.GetDatabase(DatabaseName);
            _steps = database.GetCollection<Step>("Steps");
            _journalEntries = database.GetCollection<JournalEntry>("StepEntries");
        }

        public async Task<List<Step>> GetStepsAsync(int page, int size)
        {
            FilterDefinition<Step> filter = FilterDefinition<Step>.Empty;
            FindOptions<Step> options = new FindOptions<Step>
            {
                BatchSize = size,
                NoCursorTimeout = false,
                Skip = page
            };

            var steps = (await _steps.FindAsync(filter, options)).ToList();

            List<Task<Step>> tasks = new List<Task<Step>>();

            foreach (var step in steps)
            {
                step.Journal = new Journal((await _journalEntries.FindAsync(je => je.SubjectId == step.Id)).ToList());
            };

            var results = await Task.WhenAll(tasks);

            return results.ToList();
        }

        public async Task<Step> InsertStepAsync(Step step)
        {
            step.CreatedOn = DateTime.UtcNow;
            await _steps.InsertOneAsync(step);
            return step;
        }
    }
}
