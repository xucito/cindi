
using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.States;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.Enums;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.Models;
using ConsensusCore.Domain.Services;
using ConsensusCore.Domain.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Persistence.State
{
    public partial class NodeStorageRepository : IShardRepository
    {
        ConcurrentDictionary<Guid, int> _lastShardOperation = new ConcurrentDictionary<Guid, int>();
        public async Task<bool> AddDataReversionRecordAsync(DataReversionRecord record)
        {
            await entitiesRepository.Insert(record);
            return true;
        }

        public async Task<bool> AddShardMetadataAsync(ShardMetadata shardMetadata)
        {
            await entitiesRepository.Insert(shardMetadata);
            return true;
        }

        public async Task<bool> AddShardWriteOperationAsync(ShardWriteOperation operation)
        {
            _lastShardOperation.AddOrUpdate(operation.Data.ShardId.Value, operation.Pos, (key, oldValue) =>
            {
                if (oldValue < operation.Pos)
                {
                    return operation.Pos;
                }
                return oldValue;
            });
            await entitiesRepository.Insert(operation);
            return true;
        }

        public async Task<SortedDictionary<int, ShardWriteOperation>> GetAllObjectShardWriteOperationAsync(Guid shardId, Guid objectId)
        {
            var result = new SortedDictionary<int, ShardWriteOperation>();
            foreach (var operation in (await entitiesRepository.GetAsync<ShardWriteOperation>(swo => swo.Data.ShardId.Value == shardId && swo.Data.Id == objectId)).ToList())
            {
                result.Add(operation.Pos, operation);
            }
            return result;
        }

        public async Task<List<ShardMetadata>> GetAllShardMetadataAsync()
        {
            return (await entitiesRepository.GetAsync<ShardMetadata>()).ToList();
        }

        public async Task<IEnumerable<ShardWriteOperation>> GetAllShardWriteOperationsAsync(Guid shardId)
        {
            return await entitiesRepository.GetAsync<ShardWriteOperation>(swo => swo.Data.ShardId.Value == shardId);
        }

        public async Task<SortedDictionary<int, ShardWriteOperation>> GetAllUnappliedOperationsAsync(Guid shardId)
        {
            var operations = await entitiesRepository.GetAsync<ShardWriteOperation>(swo => swo.Data.Id == shardId && swo.Applied == false);
            var result = new SortedDictionary<int, ShardWriteOperation>();
            foreach (var operation in operations)
            {
                result.Add(operation.Pos, operation);
            }
            return result;
        }

        public async Task<ShardMetadata> GetShardMetadataAsync(Guid shardId)
        {
            return await entitiesRepository.GetFirstOrDefaultAsync<ShardMetadata>(lsm => lsm.ShardId == shardId);
        }

        public async Task<ShardWriteOperation> GetShardWriteOperationAsync(Guid shardId, int syncPos)
        {
            return await entitiesRepository.GetFirstOrDefaultAsync<ShardWriteOperation>(lsm => lsm.Data.ShardId == shardId && lsm.Pos == syncPos);
        }

        public async Task<ShardWriteOperation> GetShardWriteOperationAsync(string transacionId)
        {
            return await entitiesRepository.GetFirstOrDefaultAsync<ShardWriteOperation>(lsm => lsm.Id == transacionId);
        }

        public async Task<SortedDictionary<int, ShardWriteOperation>> GetShardWriteOperationsAsync(Guid shardId, int from, int to)
        {
            var operations = await entitiesRepository.GetAsync<ShardWriteOperation>(swo => swo.Data.ShardId == shardId && swo.Pos >= from && swo.Pos <= to, null, null);
            var result = new SortedDictionary<int, ShardWriteOperation>();
            foreach (var operation in operations)
            {
                result.Add(operation.Pos, operation);
            }
            return result;
        }

        public async Task<IEnumerable<ShardWriteOperation>> GetShardWriteOperationsAsync(Guid shardId, ShardOperationOptions option, int limit)
        {
            return (await entitiesRepository.GetAsync<ShardWriteOperation>(lsm => lsm.Operation == option && lsm.Data.ShardId == shardId, null, "TransactionDate:-1", limit));
        }

        public int GetLastShardWriteOperationPos(Guid shardId)
        {
            int lastValue;

            if (_lastShardOperation.TryGetValue(shardId, out lastValue))
            {
                return lastValue;
            }

            var lastOperation = entitiesRepository.GetAsync<ShardWriteOperation>(c => c.Data.ShardId == shardId, null, "Pos:-1", 1).GetAwaiter().GetResult();
            if (lastOperation != null && lastOperation.Count() > 0)
                return (int)lastOperation.First().Pos;
            else
                return 0;
        }


        public async Task<bool> MarkShardWriteOperationAppliedAsync(string operationId)
        {
            var entity = await entitiesRepository.GetFirstOrDefaultAsync<ShardWriteOperation>(e => e.Id == operationId);
            entity.Applied = true;
            await entitiesRepository.Update<ShardWriteOperation>(entity);
            return true;
        }

        public async Task<bool> RemoveShardWriteOperationAsync(Guid shardId, int pos)
        {
            var result = await entitiesRepository.Delete<ShardWriteOperation>(d => d.Pos == pos && d.Data.ShardId == shardId);
            return true;
        }

        public async Task<bool> DeleteShardWriteOperationsAsync(List<string> shardWriteOperations)
        {
            await entitiesRepository.Delete<ShardWriteOperation>(d => shardWriteOperations.Contains(d.Id));
            return true;
        }

        public async Task<IEnumerable<ObjectDeletionMarker>> GetQueuedDeletions(Guid shardId, int toPos)
        {
            return await entitiesRepository.GetAsync<ObjectDeletionMarker>(odm => odm.ShardId == shardId && odm.Pos <= toPos);
        }

        public async Task<bool> RemoveQueuedDeletions(Guid shardId, List<Guid> objectIds)
        {
            await entitiesRepository.Delete<ObjectDeletionMarker>(odm => objectIds.Contains(odm.Id));
            return true;
        }

        public async Task<bool> MarkObjectForDeletionAsync(ObjectDeletionMarker marker)
        {
            await entitiesRepository.Insert(marker);
            return true;
        }
    }
}
