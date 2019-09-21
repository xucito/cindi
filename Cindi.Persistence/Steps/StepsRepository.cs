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
using Cindi.Domain.Exceptions.JournalEntries;
using System.Linq.Expressions;

namespace Cindi.Persistence.Steps
{
    public class StepsRepository : BaseRepository, IStepsRepository
    {
        private IMongoCollection<Step> _steps;
        //private IMongoCollection<StepMetadata> _stepMetadata;
        private IMongoCollection<JournalEntry> _journalEntries;

        public long CountSteps(string status = null)
        {
            if (status == null)
            {
                return _steps.EstimatedDocumentCount();
            }
            return _steps.Find(sm => sm.Status == status).CountDocuments();
        }

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
            //  _stepMetadata = database.GetCollection<StepMetadata>("_Steps");
            _journalEntries = database.GetCollection<JournalEntry>("JournalEntries");
        }

        public async Task<List<Step>> GetStepsAsync(int size = 10, int page = 0, string status = null, string[] stepTemplateIds = null, List<Expression<Func<Step, object>>> exclusions = null)
        {
            if (status != null && !StepStatuses.IsValid(status))
            {
                throw new InvalidStepStatusInputException(status + " is not a valid step status entry.");
            }

            var builder = Builders<Step>.Filter;
            var filters = new List<FilterDefinition<Step>>();
            var ignoreFilters = true;

            if (stepTemplateIds != null)
            {
                filters.Add(builder.In("StepTemplateId", stepTemplateIds));
                ignoreFilters = false;
            }
            if (status != null)
            {
                filters.Add(builder.Eq("Status", status));
                ignoreFilters = false;
            }

            var stepFilter = builder.And(filters);

            if (ignoreFilters)
            {
                stepFilter = FilterDefinition<Step>.Empty; ;
            }

            var sort = Builders<Step>.Sort.Descending("CreatedOn");
            FindOptions<Step> options = new FindOptions<Step>
            {
                BatchSize = size,
                NoCursorTimeout = false,
                Skip = page,
                Limit = size,
                Sort = sort
            };

            if (exclusions != null)
            {
                foreach (var exclusion in exclusions)
                {
                    options.Projection = Builders<Step>.Projection.Exclude(exclusion);
                }
            }

            return (await _steps.FindAsync(stepFilter, options)).ToList();
        }

        public async Task<List<Step>> GetStepsAsync(Guid[] stepIds)
        {
            var builder = Builders<Step>.Filter;
            var stepFilter = builder.In("Id", stepIds);
            var steps = (await _steps.FindAsync(stepFilter)).ToList();

            return steps;
        }

        public async Task<Step> GetStepAsync(Guid stepId)
        {
            var step = (await _steps.FindAsync(s => s.Id == stepId)).FirstOrDefault();
            return step;
        }


        public async Task<Step> InsertStepAsync(Step step)
        {
            await _steps.InsertOneAsync(step);
            return step;
        }

        public async Task<Step> UpdateStep(Step step)
        {
            var result = await _steps.ReplaceOneAsync(
                  doc => doc.Id == step.Id,
                  step,
                  new UpdateOptions
                  {
                      IsUpsert = false
                  });

            if (result.IsAcknowledged)
            {
                return step;
            }
            else
            {
                throw new StepUpdateFailureException("Update of step " + step.Id + " failed.");
            }
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
    }
}
