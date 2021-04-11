using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using System.Threading.Tasks;

namespace Cindi.Application.Interfaces
{
    public interface IAssignmentCache
    {
        int UnassignedCount { get; }

        Step GetNext(string[] stepTemplateIds);
        StepTemplate GetStepTemplate(string referenceId);
        Task<bool> RefreshCache();
        void Start();
    }
}