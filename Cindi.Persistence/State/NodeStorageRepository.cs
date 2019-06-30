using Cindi.Application.Interfaces;
using ConsensusCore.Domain.Services;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.State
{
    public class NodeStorageRepository : IStateRepository, INodeStorageRepository
    {
        public string DatabaseName { get; } = "CindiDb";
        public bool DoesStateExist = false;

        public IMongoCollection<NodeStorage> _clusterState;

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
            _clusterState = database.GetCollection<NodeStorage>("NodeStorageRepository");
        }

        public NodeStorage LoadNodeData()
        {
            var result = _clusterState.Find(_ => true).FirstOrDefault();
            return result;
        }

        public void SaveNodeData(NodeStorage storage)
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
                var filter = Builders<NodeStorage>.Filter.Eq("_id", storage.Id);
                _clusterState.ReplaceOne(filter, storage);
            }
        }
    }
}
