
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

namespace Cindi.Persistence.State
{
    public class NodeStorageRepository : IStateRepository, INodeStorageRepository, IShardRepository
    {
        public string DatabaseName { get; } = "CindiDb";
        public bool DoesStateExist = false;

        public IMongoCollection<NodeStorage<CindiClusterState>> _clusterState;
        public IMongoCollection<DataReversionRecord> _dataReversionRecords;
        public IMongoCollection<LocalShardMetaData> _localShardMetadata;
        public IMongoCollection<ShardOperation> _shardOperations;
        public IMongoCollection<ObjectDeletionMarker> _objectDeletionMarkers;

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
            _localShardMetadata = database.GetCollection<LocalShardMetaData>("LocalShardMetadata");
            _shardOperations = database.GetCollection<ShardOperation>("ShardOperations");
            _objectDeletionMarkers = database.GetCollection<ObjectDeletionMarker>("ObjectDeletionMarker");
        }

        public NodeStorage<CindiClusterState> LoadNodeData()
        {
            var result = _clusterState.Find(_ => true).FirstOrDefault();
            return result;
        }

        public void SaveNodeData(NodeStorage<CindiClusterState> storage)
        {
            try
            {
                if (!DoesStateExist)
                {
                    DoesStateExist = (LoadNodeData() != null);
                }
                if (!DoesStateExist)
                {
                    _clusterState.InsertOne(storage);
                }
                else
                {
                    //var filter = Builders<NodeStorage>.Filter.Eq("Id", storage.Id.ToString());
                    _clusterState.ReplaceOne(f => true, storage);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Collection was modified"))
                {

                }
                else
                {
                    Console.WriteLine("Failed to save state with error message " + e.Message + " with stack trace " + Environment.NewLine + e.StackTrace);
                }
            }
        }

        public int GetTotalShardOperationsCount(Guid shardId)
        {
            return (int)_shardOperations.CountDocuments(c => c.ShardId == shardId);
        }

        public bool AddShardOperation(ShardOperation operation)
        {
            _shardOperations.InsertOne(operation);
            return true;
        }

        public bool RemoveShardOperation(Guid shardId, int pos)
        {
            var result = _shardOperations.DeleteOne(d => d.Pos == pos && d.ShardId == shardId);
            return result.IsAcknowledged;
        }

        public bool AddDataReversionRecord(DataReversionRecord record)
        {
            _dataReversionRecords.InsertOne(record);
            return true;
        }

        public bool IsObjectMarkedForDeletion(Guid shardId, Guid objectId)
        {
            return _objectDeletionMarkers.CountDocuments(odm => odm.ObjectId == objectId && odm.ShardId == shardId) > 0;
        }

        public bool MarkObjectForDeletion(ObjectDeletionMarker marker)
        {
            _objectDeletionMarkers.InsertOne(marker);
            return true;
        }

        public bool AddShardMetadata(LocalShardMetaData shardMetadata)
        {
            _localShardMetadata.InsertOne(shardMetadata);
            return true;
        }

        public bool UpdateShardMetadata(LocalShardMetaData shardMetadata)
        {
            return _localShardMetadata.ReplaceOne(lsm => lsm.ShardId == shardMetadata.ShardId && lsm.SyncPos == shardMetadata.SyncPos, shardMetadata).IsAcknowledged;
        }

        public bool UpdateShardOperation(Guid shardId, ShardOperation operation)
        {
            return _shardOperations.ReplaceOne(so => so.ShardId == shardId && operation.Pos == so.Pos, operation).IsAcknowledged;
        }

        public LocalShardMetaData GetShardMetadata(Guid shardId)
        {
            return _localShardMetadata.Find(lsm => lsm.ShardId == shardId).FirstOrDefault();
        }

        public bool ShardMetadataExists(Guid shardId)
        {
            return _localShardMetadata.Find(lsm => lsm.ShardId == shardId).FirstOrDefault() != null;
        }

        public ShardOperation GetShardOperation(Guid shardId, int syncPos)
        {
            return _shardOperations.Find(lsm => lsm.ShardId == shardId && lsm.Pos == syncPos).FirstOrDefault();
        }

        public IEnumerable<ShardOperation> GetAllShardOperations(Guid shardId)
        {
            return _shardOperations.Find(so => so.ShardId == shardId).ToEnumerable();
        }

        public IEnumerable<ShardOperation> GetAllUncommitedOperations(Guid shardId)
        {
            return _shardOperations.Find(so => so.ShardId == shardId && !so.Applied).ToEnumerable();
        }
    }
}
