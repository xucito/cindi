using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Entities.Steps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cindi.Application.Interfaces
{
    public interface IStepsRepository
    {
        long CountSteps(string status = null);
        Task<List<Step>> GetStepsAsync(int size = 10, int page = 0, string status = null, string[] stepTemplateIds = null);
        Task<Step> GetStepAsync(Guid stepId);
        Task<Step> InsertStepAsync(Step step);
        Task<Step> UpdateStep(Step step);
    }
}