using Cindi.Application.Services.ClusterState;
using System.Threading.Tasks;

namespace Cindi.Application.Interfaces
{
    public interface IClusterRepository
    {
        Task<bool> SaveClusterState(ClusterState state);
        Task<ClusterState> GetClusterState();
    }
}