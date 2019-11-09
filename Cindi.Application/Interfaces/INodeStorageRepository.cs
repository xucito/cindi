using Cindi.Domain.Entities.States;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.Services;

namespace Cindi.Application.Interfaces
{
    public interface INodeStorageRepository: IBaseRepository<CindiClusterState>, IShardRepository
    {
        string DatabaseName { get; }

        NodeStorage<CindiClusterState> LoadNodeData();
        void SaveNodeData(NodeStorage<CindiClusterState> storage);
    }
}