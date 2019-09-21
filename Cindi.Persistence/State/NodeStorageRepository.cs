
using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.States;
using ConsensusCore.Domain.Services;
using Microsoft.Extensions.Logging;
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

        public IMongoCollection<NodeStorage<CindiClusterState>> _clusterState;

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
    }
}
