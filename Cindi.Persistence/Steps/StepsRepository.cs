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

        public long CountSteps() { return _steps.EstimatedDocumentCount(); }

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

        public async Task<List<Step>> GetStepsAsync(int page = 0, int size = 10)
        {
            FilterDefinition<Step> filter = FilterDefinition<Step>.Empty;
            FindOptions<Step> options = new FindOptions<Step>
            {
                BatchSize = size,
                NoCursorTimeout = false,
                Skip = page,
                Limit = size
            };

            var steps = (await _steps.FindAsync(filter, options)).ToList();

            List<Task<Step>> tasks = new List<Task<Step>>();

            var builder = Builders<JournalEntry>.Filter;

            var journalFilter = builder.In("SubjectId", steps.Select(s => s.Id));

            var journals = (await _journalEntries.FindAsync(journalFilter)).ToList();
            foreach (var step in steps)
            {
                step.Journal = new Journal(journals.Where(j => j.SubjectId == step.Id).ToList());
              /*  tasks.Add(Task.Run(async () =>
                {
                    var temp = step;
                    temp.Journal = new Journal((await _journalEntries.FindAsync(je => je.SubjectId == step.Id)).ToList());
                    return temp;
                }));
                */
            };
            
            return steps;
        }

        public async Task<Step> GetStepAsync(Guid stepId)
        {
            var step = (await _steps.FindAsync(s => s.Id == stepId)).FirstOrDefault();
            step.Journal = new Journal((await _journalEntries.FindAsync(je => je.SubjectId == step.Id)).ToList());
            return step;
        }


        public async Task<Step> InsertStepAsync(Step step)
        {
            step.Id = Guid.NewGuid();
            step.CreatedOn = DateTime.UtcNow;
            await _steps.InsertOneAsync(step);
            return step;
        }

        public async Task<JournalEntry> InsertJournalEntryAsync(JournalEntry entry)
        {
            await _journalEntries.InsertOneAsync(entry);
            return entry;
        }
    }
}
