
using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.States;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.Models;
using ConsensusCore.Domain.Services;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Persistence.State
{
    public partial class NodeStorageRepository : IShardRepository
    {
        public async Task<bool> AddDataReversionRecordAsync(DataReversionRecord record)
        {
            await _dataReversionRecords.InsertOneAsync(record);
            return true;
        }

        public async Task<bool> AddShardMetadataAsync(ShardMetadata shardMetadata)
        {
            await _localShardMetadata.InsertOneAsync(shardMetadata);
            return true;
        }

        public async Task<bool> AddShardWriteOperationAsync(ShardWriteOperation operation)
        {
            await _shardWriteOperations.InsertOneAsync(operation);
            return true;
        }

        public async Task<SortedDictionary<int, ShardWriteOperation>> GetAllObjectShardWriteOperationAsync(Guid shardId, Guid objectId)
        {
            var result = new SortedDictionary<int, ShardWriteOperation>();
            foreach (var operation in (await _shardWriteOperations.FindAsync(swo => swo.Data.ShardId.Value == shardId && swo.Data.Id == objectId)).ToList())
            {
                result.Add(operation.Pos, operation);
            }
            return result;
        }

        public async Task<List<ShardMetadata>> GetAllShardMetadataAsync()
        {
            return await _localShardMetadata.Find(_ => true).ToListAsync();
        }

        public async Task<IEnumerable<ShardWriteOperation>> GetAllShardWriteOperationsAsync(Guid shardId)
        {
            return await _shardWriteOperations.Find(swo => swo.Data.ShardId.Value == shardId).ToListAsync();
        }

        public async Task<SortedDictionary<int, ShardWriteOperation>> GetAllUnappliedOperationsAsync(Guid shardId)
        {
            var operations = await _shardWriteOperations.Find(swo => swo.Data.Id == shardId && swo.Applied == false).ToListAsync();
            var result = new SortedDictionary<int, ShardWriteOperation>();
            foreach (var operation in operations)
            {
                result.Add(operation.Pos, operation);
            }
            return result;
        }

        public async Task<ShardMetadata> GetShardMetadataAsync(Guid shardId)
        {
            return await _localShardMetadata.Find(lsm => lsm.ShardId == shardId).FirstOrDefaultAsync();
        }

        public async Task<ShardWriteOperation> GetShardWriteOperationAsync(Guid shardId, int syncPos)
        {
            return await _shardWriteOperations.Find(lsm => lsm.Data.ShardId == shardId && lsm.Pos == syncPos).FirstOrDefaultAsync();
        }

        public async Task<ShardWriteOperation> GetShardWriteOperationAsync(string transacionId)
        {
            return await _shardWriteOperations.Find(lsm => lsm.Id == transacionId).FirstOrDefaultAsync();
        }

        public async Task<SortedDictionary<int, ShardWriteOperation>> GetShardWriteOperationsAsync(Guid shardId, int from, int to)
        {
            var operations = await _shardWriteOperations.Find(swo => swo.Data.ShardId == shardId && swo.Pos >= from && swo.Pos <= to).ToListAsync();
            var result = new SortedDictionary<int, ShardWriteOperation>();
            foreach (var operation in operations)
            {
                result.Add(operation.Pos, operation);
            }
            return result;
        }

        public int GetTotalShardWriteOperationsCount(Guid shardId)
        {
            return (int)_shardWriteOperations.CountDocuments(c => c.Data.ShardId == shardId);
        }

        public bool IsObjectMarkedForDeletion(Guid shardId, Guid objectId)
        {
            return _objectDeletionMarkers.CountDocuments(odm => odm.ObjectId == objectId && odm.ShardId == shardId) > 0;
        }

        public async Task<bool> MarkObjectForDeletionAsync(ObjectDeletionMarker marker)
        {
            await _objectDeletionMarkers.InsertOneAsync(marker);
            return true;
        }

        public async Task<bool> MarkShardWriteOperationAppliedAsync(string operationId)
        {
            var update = Builders<ShardWriteOperation>.Update.Set("Applied", true);
            return (await _shardWriteOperations.UpdateOneAsync(swo => swo.Id == operationId, update)).IsAcknowledged;
        }

        public async Task<bool> RemoveShardWriteOperationAsync(Guid shardId, int pos)
        {
            var result = await _shardWriteOperations.DeleteOneAsync(d => d.Pos == pos && d.Data.ShardId == shardId);
            return result.IsAcknowledged;
        }
    }
}
