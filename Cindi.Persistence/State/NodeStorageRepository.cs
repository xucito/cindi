
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
    public partial class NodeStorageRepository
    {
        public string DatabaseName { get; } = "CindiDb";
        public bool DoesStateExist = false;

        public IMongoCollection<NodeStorage<CindiClusterState>> _clusterState;
        public IMongoCollection<DataReversionRecord> _dataReversionRecords;
        public IMongoCollection<ShardMetadata> _localShardMetadata;
        public IMongoCollection<ShardWriteOperation> _shardWriteOperations;
        public IMongoCollection<ObjectDeletionMarker> _objectDeletionMarkers;
        public IMongoCollection<ShardWriteOperation> _shardWriteOperationsQueue;
        public IMongoCollection<ShardWriteOperation> _shardWriteOperationsTransit;

        public NodeStorageRepository(string mongoDbConnectionString, string databaseName)
        {
            var client = new MongoClient(mongoDbConnectionString);
            SetCollection(client);
        }

        public NodeStorageRepository(IMongoClient client)
        {
            SetCollection(client);
        }

        private void SetCollection(IMongoClient client)
        {
            var database = client.GetDatabase(DatabaseName);
            _clusterState = database.GetCollection<NodeStorage<CindiClusterState>>("NodeStorageRepository");
            _dataReversionRecords = database.GetCollection<DataReversionRecord>("DataReversionRecords");
            _localShardMetadata = database.GetCollection<ShardMetadata>("ShardMetadata");
            _shardWriteOperations = database.GetCollection<ShardWriteOperation>("ShardWriteOperations");
            _objectDeletionMarkers = database.GetCollection<ObjectDeletionMarker>("ObjectDeletionMarker");
            _shardWriteOperationsQueue = database.GetCollection<ShardWriteOperation>("ShardWriteOperationsQueue");
            _shardWriteOperationsTransit = database.GetCollection<ShardWriteOperation>("ShardWriteOperationsTransit");
        }
    }
}
