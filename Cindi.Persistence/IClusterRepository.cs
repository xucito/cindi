using Cindi.Application.Utilities;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.RPCs.Shard;
using System.Threading.Tasks;

namespace Cindi.Persistence
{
    public interface IClusterRepository
    {
        Task<AddShardWriteOperationResponse> AddWriteOperation<T>(WriteEntityOperation<T> request) where T : ShardData;
    }
}