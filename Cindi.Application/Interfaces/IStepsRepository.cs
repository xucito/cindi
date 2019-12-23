using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Entities.Steps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Cindi.Domain.Enums;

namespace Cindi.Application.Interfaces
{
    public interface IStepsRepository
    {
        long CountSteps(string status = null);
        Task<List<Step>> GetStepsAsync(int size = 10, int page = 0, string status = null, string[] stepTemplateIds = null, List<Expression<Func<Step, object>>> exclusions = null, SortOrder order = SortOrder.Descending, string sortField = "CreatedOn");
        Task<IEnumerable<Step>> GetStepsAsync(DateTime fromDate, DateTime toDate);
        Task<Step> GetStepAsync(Guid stepId);
        Task<Step> InsertStepAsync(Step step);
        Task<Step> UpdateStep(Step step);
    }
}