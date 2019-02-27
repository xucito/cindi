using Cindi.Application.Interfaces;
using Cindi.Application.Services.ClusterState;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Persistence.Cluster
{
    public class ClusterRepository : BaseRepository, IClusterRepository
    {
        private IMongoCollection<ClusterState> _state;
        
        public ClusterRepository(string mongoDbConnectionString, string databaseName) : base(mongoDbConnectionString, databaseName)
        {
            var client = new MongoClient(mongoDbConnectionString);
            SetCollection(client);
        }

        public ClusterRepository(IMongoClient client) : base(client)
        {
            SetCollection(client);
        }

        private void SetCollection(IMongoClient client)
        {
            var database = client.GetDatabase(DatabaseName);
            _state = database.GetCollection<ClusterState>("ClusterState");
        }

        public async Task<ClusterState> GetClusterState()
        {
            var result = (await _state.FindAsync(s => true)).ToList();

            if(result.Count == 1)
            {
                return result.First();
            }
            else if(result.Count > 1)
            {
                throw new Exception("Conflicting state found.");
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> SaveClusterState(ClusterState state)
        {
            var replaceResult = await _state.ReplaceOneAsync(
                doc => doc.Id == ClusterState.DefaultId,
                state,
                new UpdateOptions { IsUpsert = true }
                );


            return replaceResult.IsAcknowledged;
        }
    }
}
