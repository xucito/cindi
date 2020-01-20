using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.Workflows;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Cindi.Persistence.Workflows
{
    public class WorkflowsRepository : BaseRepository, IWorkflowsRepository
    {
        public IMongoCollection<Workflow> _workflows;
        private IMongoCollection<JournalEntry> _workflowsJournalEntries;
        private IMongoCollection<JournalEntry> _stepJournalEntries;
        private IMongoCollection<Step> _steps;
        private IMongoCollection<WorkflowMetadata> _workflowsMetadata;

        public WorkflowsRepository(string mongoDbConnectionString, string databaseName) : base(mongoDbConnectionString, databaseName)
        {
            var client = new MongoClient(mongoDbConnectionString);
            SetCollection(client);
        }

        public WorkflowsRepository(IMongoClient client) : base(client)
        {
            SetCollection(client);
        }

        public async Task<List<Step>> GetWorkflowStepsAsync(Guid workflowId)
        {
            var steps = (await _steps.FindAsync(s => s.WorkflowId != null && s.WorkflowId.Value == workflowId)).ToList();

            return steps;
        }

        private void SetCollection(IMongoClient client)
        {
            var database = client.GetDatabase(DatabaseName);
            _workflows = database.GetCollection<Workflow>("Workflows");
            _workflowsJournalEntries = database.GetCollection<JournalEntry>("JournalEntries");
            _steps = database.GetCollection<Step>("Steps");
            _stepJournalEntries = database.GetCollection<JournalEntry>("JournalEntries");
            _workflowsMetadata = database.GetCollection<WorkflowMetadata>("_Workflows");
        }

        public long CountWorkflows() { return _workflows.EstimatedDocumentCount(); }

        public async Task<Workflow> InsertWorkflowAsync(Workflow workflow)
        {
            await _workflows.InsertOneAsync(workflow);
            await UpsertWorkflowMetadataAsync(workflow.Id);
            return workflow;
        }

        public async Task<Workflow> GetWorkflowAsync(Guid WorkflowId)
        {
            var foundWorkflow = (await _workflows.FindAsync(s => s.Id == WorkflowId)).FirstOrDefault();
            return foundWorkflow;
        }

        public async Task<List<Workflow>> GetWorkflowsAsync(Guid[] workflowIds, List<Expression<Func<Workflow, object>>> exclusions = null)
        {
            var builder = Builders<Workflow>.Filter;
            var workflowFilter = builder.In("Id", workflowIds);

            FindOptions<Workflow> options = new FindOptions<Workflow>
            {
                NoCursorTimeout = false,
                Skip = 0,
            };

            if (exclusions != null)
            {
                foreach (var exclusion in exclusions)
                {
                    options.Projection = Builders<Workflow>.Projection.Exclude(exclusion);
                }
            }

            var workflows = (await _workflows.FindAsync(workflowFilter, options)).ToList();

            return workflows;
        }

        public async Task<Workflow> UpdateWorkflow(Workflow workflow)
        {
            var result = await _workflows.ReplaceOneAsync(
                  doc => doc.Id == workflow.Id,
                  workflow,
                  new UpdateOptions
                  {
                      IsUpsert = false
                  });

            await UpsertWorkflowMetadataAsync(workflow.Id);

            if (result.IsAcknowledged)
            {
                return workflow;
            }
            else
            {
                throw new WorkflowUpdateFailureException("Update of workflow " + workflow.Id + " failed.");
            }
        }

        public async Task<List<Workflow>> GetWorkflowsAsync(int size = 10, int page = 0, string status = null, string[] WorkflowTemplateIds = null, List<Expression<Func<Workflow, object>>> exclusions = null)
        {
            if (status != null && !WorkflowStatuses.IsValid(status))
            {
                throw new InvalidWorkflowStatusException(status + " is not a valid step status entry.");
            }

            var builder = Builders<WorkflowMetadata>.Filter;
            var filters = new List<FilterDefinition<WorkflowMetadata>>();
            var ignoreFilters = true;

            if (WorkflowTemplateIds != null)
            {
                filters.Add(builder.In("WorkflowId", WorkflowTemplateIds));
                ignoreFilters = false;
            }
            if (status != null)
            {
                filters.Add(builder.Eq("Status", status));
                ignoreFilters = false;
            }

            var stepMetadataFilter = builder.And(filters);

            if (ignoreFilters)
            {
                stepMetadataFilter = FilterDefinition<WorkflowMetadata>.Empty; ;
            }

            var sort = Builders<WorkflowMetadata>.Sort.Ascending("CreatedOn");

            FindOptions<WorkflowMetadata> options = new FindOptions<WorkflowMetadata>
            {
                BatchSize = size,
                NoCursorTimeout = false,
                Skip = page,
                Limit = size,
                Sort = sort
            };


            var validWorkflows = (await _workflowsMetadata.FindAsync(stepMetadataFilter, options)).ToList();

            return await GetWorkflowsAsync(validWorkflows.Select(vs => vs.WorkflowId).ToArray(), exclusions);
        }

        /// <summary>
        /// The id will be the same
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        private async Task<WorkflowMetadata> UpsertWorkflowMetadataAsync(Guid workflowId)
        {
            var workflowToUpdate = await GetWorkflowAsync(workflowId);

            var replaceResult = await _workflowsMetadata.ReplaceOneAsync(
                    doc => doc.WorkflowId == workflowId,
                    workflowToUpdate.Metadata,
                    new UpdateOptions { IsUpsert = true }
                    );

            return workflowToUpdate.Metadata;
        }
    }
}
