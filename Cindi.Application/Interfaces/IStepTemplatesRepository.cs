using System.Collections.Generic;
using System.Threading.Tasks;
using Cindi.Domain.Entities.StepTemplates;

namespace Cindi.Application.Interfaces
{
    public interface IStepTemplatesRepository
    {
        Task<StepTemplate> GetStepTemplateAsync(string name, string version);
        Task<List<StepTemplate>> GetStepTemplatesAsync(int page, int size);
        Task<StepTemplate> InsertAsync(StepTemplate stepTemplate);
        long CountStepTemplates();
    }
}