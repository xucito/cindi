using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Entities.Steps;

namespace Cindi.Application.Interfaces
{
    public interface IWorkflowsRepository
    {
        long CountWorkflows();
        Task<Workflow> GetWorkflowAsync(Guid WorkflowId);
        Task<List<Workflow>> GetWorkflowsAsync(int size = 10, int page = 0, string status = null, string[] WorkflowTemplateIds = null);
        Task<Workflow> InsertWorkflowAsync(Workflow Workflow);
        Task<List<Step>> GetWorkflowStepsAsync(Guid workflowId);
        Task<Workflow> UpdateWorkflow(Workflow Workflow);
    }
}