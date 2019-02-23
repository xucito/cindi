using Cindi.Domain.Entities.Steps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cindi.Application.Interfaces
{
    public interface IStepsRepository
    {
        Task<List<Step>> GetStepsAsync(int page, int size);
    }
}