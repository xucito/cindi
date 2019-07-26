using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Domain.Entities.StepTemplates;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Persistence.WorkflowTemplates
{
    public class WorkflowTemplatesRepository : BaseRepository, IWorkflowTemplatesRepository
    {
        public IMongoCollection<WorkflowTemplate> _workflowTemplate;

        public WorkflowTemplatesRepository(string mongoDbConnectionString, string databaseName) : base(mongoDbConnectionString, databaseName)
        {
            var client = new MongoClient(mongoDbConnectionString);
            SetCollection(client);
        }
        
        public WorkflowTemplatesRepository(IMongoClient client) : base(client)
        {
            SetCollection(client);
        }

        private void SetCollection(IMongoClient client)
        {
            var database = client.GetDatabase(DatabaseName);
            _workflowTemplate = database.GetCollection<WorkflowTemplate>("WorkflowTemplates");
        }

        public long CountWorkflowTemplates() { return _workflowTemplate.EstimatedDocumentCount(); }

        public async Task<WorkflowTemplate> InsertWorkflowTemplateAsync(WorkflowTemplate workflowTemplate)
        {
            await _workflowTemplate.InsertOneAsync(workflowTemplate);
            return workflowTemplate;
        }

        public async Task<WorkflowTemplate> GetWorkflowTemplateAsync(string WorkflowTemplateId)
        {
            var foundWorkflowTemplate = (await _workflowTemplate.FindAsync(s => s.ReferenceId == WorkflowTemplateId)).FirstOrDefault();
            return foundWorkflowTemplate;
        }

        public async Task<WorkflowTemplate> GetWorkflowTemplateAsync(Guid id)
        {
            var foundWorkflowTemplate = (await _workflowTemplate.FindAsync(s => s.Id == id)).FirstOrDefault();
            return foundWorkflowTemplate;
        }

        public async Task<List<WorkflowTemplate>> GetWorkflowTemplatesAsync(int page = 0, int size = 10)
        {
            FilterDefinition<WorkflowTemplate> filter = FilterDefinition<WorkflowTemplate>.Empty;
            FindOptions<WorkflowTemplate> options = new FindOptions<WorkflowTemplate>
            {
                BatchSize = size,
                NoCursorTimeout = false,
                Skip = page,
                Limit = size
            };

            var workflows = (await _workflowTemplate.FindAsync(filter, options)).ToList();

            return workflows;
        }
    }
}
