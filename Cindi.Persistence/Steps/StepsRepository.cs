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
using Cindi.Domain.ValueObjects;
using MongoDB.Bson;
using Cindi.Domain.Exceptions.Steps;

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

        public async Task<Step> GetSteps(string status, string[] stepTemplateIds)
        {
            if (!StepStatuses.IsValid(status))
            {
                throw new InvalidStepStatusInputException(status + " is not a valid step status entry.");
            }

            var filter = Builders<Step>.Filter.In(x => x.StepTemplateId, stepTemplateIds);
            var matchingSteps =  _steps.Find(filter).ToEnumerable();

            //Add cursor here
            var batch = new List<Step>();
            var page = 0;
            var size = 100;
            var totalRecordCount = matchingSteps.Count();
            var searchedRecord = 0;

            while (searchedRecord < totalRecordCount)
            {
                batch = matchingSteps.Skip(page).Take(size).ToList();
                searchedRecord += batch.Count();
                var projectStatus = new BsonDocument[] {

            new BsonDocument {
                { "$match", new BsonDocument("$expr", new BsonDocument{ { "$in", new BsonArray { "$SubjectId", new BsonArray(batch.Select(s => s.Id)) } } } )}  } ,
                new BsonDocument {{ "$project", new BsonDocument(new Dictionary<string, object> {
                { "_id", "$SubjectId" } ,
                 { "SubjectId", "$SubjectId" } ,
                { "Updates", new BsonDocument {{"$filter",
                        new BsonDocument { {"input", "$Updates" },
                            { "as","item"},
                            { "cond", new BsonDocument() { { "$eq", new BsonArray() {
                                "$$item.FieldName",
                                "status"
                } } } } } } } },
                { "RecordedOn", "$RecordedOn" }
            }) } } ,

            new BsonDocument{{"$sort", new BsonDocument("RecordedOn",1)}},
            new BsonDocument{{"$group", new BsonDocument{
                { "_id","$SubjectId" },
                { "Status", new BsonDocument("$last","$Updates")},
                {"SubjectId", new BsonDocument("$last", "$SubjectId") }
            } } },
            new BsonDocument{{ "$match", new BsonDocument("Status.0.Value", status) }},
            };

                var createBuckets = new BsonDocument { { "$bucket", new BsonDocument { { "groupBy", "$SubjectId" }, { "boundaries", new BsonArray() { 0, 5, 10 } } } } };

                var result = _journalEntries.Aggregate<JournalEntry>(projectStatus).ToList();//.AppendStage<JournalEntry>(projectStatus).ToList();

                if(result.Count() > 0)
                {
                    //Always take the first element
                    var foundStep = batch.Where(s => s.Id == result.First().SubjectId).First();
                    //Get full journal
                    foundStep.Journal = new Journal(_journalEntries.Find(je => je.SubjectId == foundStep.Id).ToList());
                    return foundStep;
                }
                page++;
            }
            return null;//result;
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
            await InsertJournalEntryAsync(new JournalEntry()
            {
                SubjectId = step.Id,
                ChainId = 0,
                Entity = JournalEntityTypes.Step,
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
            await _steps.InsertOneAsync(step);
            return step;
        }

        public async Task<JournalEntry> InsertJournalEntryAsync(JournalEntry entry)
        {
            entry.RecordedOn = DateTime.UtcNow;
            await _journalEntries.InsertOneAsync(entry);
            return entry;
        }
    }
}
