using Cindi.Application.Services.ClusterState;
using System.Threading.Tasks;

namespace Cindi.Persistence.SequenceTemplates
{
    public interface IClusterRepository
    {
        Task<bool> SaveClusterState(ClusterState state);
        Task<ClusterState> GetClusterState();
    }
}