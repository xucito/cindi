using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cindi.Domain.Entities.WorkflowsTemplates;

namespace Cindi.Application.Interfaces
{
    public interface IWorkflowTemplatesRepository
    {
        Task<WorkflowTemplate> GetWorkflowTemplateAsync(string WorkflowTemplateId);
        Task<WorkflowTemplate> GetWorkflowTemplateAsync(Guid id);
        Task<List<WorkflowTemplate>> GetWorkflowTemplatesAsync(int page = 0, int size = 10);
        Task<WorkflowTemplate> InsertWorkflowTemplateAsync(WorkflowTemplate workflowTemplate);
        long CountWorkflowTemplates();
    }
}