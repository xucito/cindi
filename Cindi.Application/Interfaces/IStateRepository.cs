using Cindi.Domain.Entities.States;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.Services;

namespace Cindi.Application.Interfaces
{
    public interface IStateRepository: ConsensusCore.Domain.Interfaces.IBaseRepository<CindiClusterState>
    {
        string DatabaseName { get; }
        NodeStorage<CindiClusterState> LoadNodeData();
        void SaveNodeData(NodeStorage<CindiClusterState> storage);
    }
}