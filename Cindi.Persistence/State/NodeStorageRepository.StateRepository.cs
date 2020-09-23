
using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.States;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.Models;
using ConsensusCore.Domain.Services;
using Microsoft.Extensions.Logging;
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
            var result = await entitiesRepository.GetFirstOrDefaultAsync<NodeStorage<CindiClusterState>>();
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
                    await entitiesRepository.Insert<NodeStorage<CindiClusterState>>(storage);
                    return true;
                }
                else
                {
                    //var filter = Builders<NodeStorage>.Filter.Eq("Id", storage.Id.ToString());
                    await entitiesRepository.Update(storage);
                    return true;
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
