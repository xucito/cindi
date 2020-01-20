
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
    public partial class NodeStorageRepository : INodeStorageRepository
    {
        public async Task<NodeStorage<CindiClusterState>> LoadNodeDataAsync()
        {
            var result = await _clusterState.Find(_ => true).FirstOrDefaultAsync();
            return result;
        }

        public async Task<bool> SaveNodeDataAsync(NodeStorage<CindiClusterState> storage)
        {
            try
            {
                if (!DoesStateExist)
                {
                    DoesStateExist = (await LoadNodeDataAsync() != null);
                }

                if (!DoesStateExist)
                {
                    await _clusterState.InsertOneAsync(storage);
                    return true;
                }
                else
                {
                    //var filter = Builders<NodeStorage>.Filter.Eq("Id", storage.Id.ToString());
                    return (await _clusterState.ReplaceOneAsync(f => true, storage)).IsAcknowledged;
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
                return false;
            }
        }
    }
}
