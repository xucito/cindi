using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.Services;

namespace Cindi.Application.Interfaces
{
    public interface INodeStorageRepository: IBaseRepository
    {
        string DatabaseName { get; }

        NodeStorage LoadNodeData();
        void SaveNodeData(NodeStorage storage);
    }
}