using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.Services;

namespace Cindi.Application.Interfaces
{
    public interface IStateRepository: ConsensusCore.Domain.Interfaces.IBaseRepository
    {
        string DatabaseName { get; }
        NodeStorage LoadNodeData();
        void SaveNodeData(NodeStorage storage);
    }
}