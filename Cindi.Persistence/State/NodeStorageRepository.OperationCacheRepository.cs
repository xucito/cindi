
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
    public partial class NodeStorageRepository : IOperationCacheRepository
    {
        public async Task<bool> AddOperationToTransitAsync(ShardWriteOperation operation)
        {
            await _shardWriteOperationsTransit.InsertOneAsync(operation);
            return true;
        }

        public int CountOperationsInQueue()
        {
            return (int)_shardWriteOperationsQueue.CountDocuments(swoq => true);
        }

        public int CountOperationsInTransit()
        {
            return (int)_shardWriteOperationsTransit.CountDocuments(swoq => true);
        }

        public async Task<bool> DeleteOperationFromQueueAsync(ShardWriteOperation operation)
        {
            return (await _shardWriteOperationsQueue.DeleteOneAsync(swoq => swoq.Id == operation.Id)).IsAcknowledged;
        }

        public async Task<bool> DeleteOperationFromTransitAsync(string operationId)
        {
            return (await _shardWriteOperationsTransit.DeleteOneAsync(swoq => swoq.Id == operationId)).IsAcknowledged;
        }

        public async Task<bool> EnqueueOperationAsync(ShardWriteOperation data)
        {
            await _shardWriteOperationsQueue.InsertOneAsync(data);
            return true;
        }

        public async Task<ShardWriteOperation> GetNextOperationAsync()
        {
            return await _shardWriteOperationsQueue.Find(d => true).SortBy(d => d.TransactionDate).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ShardWriteOperation>> GetOperationQueueAsync()
        {
            return await _shardWriteOperationsQueue.Find(_ => true).ToListAsync();
        }

        public async Task<IEnumerable<ShardWriteOperation>> GetTransitQueueAsync()
        {
            return await _shardWriteOperationsTransit.Find(_ => true).ToListAsync();
        }

        public bool IsOperationInQueue(string operationId)
        {
            return (_shardWriteOperationsQueue.Find(d => d.Id == operationId).FirstOrDefault()) != null;
        }

        public bool IsOperationInTransit(string operationId)
        {
            return (_shardWriteOperationsTransit.Find(d => d.Id == operationId).FirstOrDefault()) != null;
        }
    }
}
