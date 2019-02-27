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

        public async Task<Step> GetStepsAsync(string status, Dictionary<string, DateTime?> stepFilters)
        {
            if (!StepStatuses.IsValid(status))
            {
                throw new InvalidStepStatusInputException(status + " is not a valid step status entry.");
            }

            //var filter = Builders<Step>.Filter.In(x => x.StepTemplateId, stepTemplateIds);

            var searchArray = new BsonArray();

            foreach (var templateId in stepFilters)
            {
                var innerArray = new BsonArray { new BsonDocument { { "StepTemplateId", templateId.Key } } };
                if (templateId.Value != null)
                {
                    innerArray.Add(new BsonDocument { { "CreatedOn", new BsonDocument { { "$gt", templateId.Value } } } });
                }
                searchArray.Add(new BsonDocument { { "$and", innerArray } });
            }

            var stepFilter = new BsonDocument { { "$or", searchArray } };


            var matchingSteps = _steps.Find(stepFilter).SortBy(s => s.CreatedOn).Project(projection => projection.Id).ToList();

            //Add cursor here
            var batch = new List<Guid>();
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
                { "$match", new BsonDocument("$expr", new BsonDocument{ { "$in", new BsonArray { "$SubjectId", new BsonArray(batch.Select(s => s)) } } } )}  } ,
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

                var result = _journalEntries.Aggregate<JournalEntry>(projectStatus).ToList().Select(je => je.SubjectId);//.AppendStage<JournalEntry>(projectStatus).ToList();

                if (result.Count() > 0)
                {
                    //Always take the first element
                    foreach (var s in batch)
                    {
                        if (result.Contains(s))
                        {
                            //Pull latest chain and if the chain is now showing unassigned do not return it
                            var fullStep = await GetStepAsync(s);
                            //s.Journal = new Journal(_journalEntries.Find(je => je.SubjectId == s.Id).ToList());
                            if (fullStep.Status == status)
                            {
                                return fullStep;
                            }
                        }
                    }
                    //Get full journal
                    //foundStep.Journal = new Journal(_journalEntries.Find(je => je.SubjectId == foundStep.Id).ToList());
                    //return foundStep;
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
            await _steps.InsertOneAsync(step);
            return step;
        }

        /*public async Task<JournalEntry> ReassignStepAsync(Guid stepId, string status)
        {
            if (!StepStatuses.IsValid(status))
            {
                throw new InvalidStepStatusInputException(status + " is not a valid staus.");
            }

            var step = await GetStepAsync(stepId);

            //Throw a error if you are assigning a step that is unassigned
            if(status == StepStatuses.Assigned && step.Status != StepStatuses.Unassigned)
            {
                throw new InvalidStepQueueException("You cannot assign step " + stepId + " as it is not unassigned.");
            }

            //Throw a error if you try to complete a step without it being in a assigned
            if(StepStatuses.IsCompleteStatus(status) && step.Status != StepStatuses.Assigned)
            {
                throw new InvalidStepQueueException("Cannot complete step with status " +  status + " when the step is not assigned first, the current status is " + step.Status);
            }

            return await InsertJournalEntryAsync(new JournalEntry() {
            });
        }*/

        public async Task<int> GetNextChainId(Guid subjectId)
        {
            var filter = Builders<JournalEntry>.Filter.Eq(x => x.SubjectId, subjectId);
            return (await _journalEntries.FindAsync(filter)).ToList().OrderBy(je => je.RecordedOn).Last().ChainId + 1;
        }

        public async Task<JournalEntry> InsertJournalEntryAsync(JournalEntry entry)
        {
            await _journalEntries.InsertOneAsync(entry);
            return entry;
        }
    }
}
