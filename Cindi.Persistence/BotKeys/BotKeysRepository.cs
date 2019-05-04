using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Exceptions.BotKeys;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Persistence.BotKeys
{
    public class BotKeysRepository : BaseRepository, IBotKeysRepository
    {
        private IMongoCollection<BotKey> _keys;

        public BotKeysRepository(string mongoDbConnectionString, string databaseName) : base(mongoDbConnectionString, databaseName)
        {
            var client = new MongoClient(mongoDbConnectionString);
            SetCollection(client);
        }

        public BotKeysRepository(IMongoClient client) : base(client)
        {
            SetCollection(client);
        }

        private void SetCollection(IMongoClient client)
        {
            var database = client.GetDatabase(DatabaseName);
            _keys = database.GetCollection<BotKey>("BotKeys");
        }

        public async Task<List<BotKey>> GetBotKeysAsync(int size = 10, int page = 0)
        {
            var builder = Builders<BotKey>.Filter;
            var filters = new List<FilterDefinition<BotKey>>();
            var keysFilter = FilterDefinition<BotKey>.Empty;
            FindOptions<BotKey> options = new FindOptions<BotKey>
            {
                BatchSize = size,
                NoCursorTimeout = false,
                Skip = page,
                Limit = size
            };

            var validUsers = (await _keys.FindAsync(keysFilter, options)).ToList();

            return validUsers;
        }

        public async Task<bool> UpdateBotKey (BotKey updatedKey)
        {
            var result = await _keys.ReplaceOneAsync((keys) => keys.Id == updatedKey.Id, updatedKey);

            if(result.IsAcknowledged)
            {
                return true;
            }
            else
            {
                throw new Exception("Bot Key update failed.");
            }
        }

        public async Task<BotKey> GetBotKeyAsync(Guid id)
        {
            var keys = await _keys.FindAsync(u => u.Id == id);
            return keys.FirstOrDefault();
        }

        public async Task<Guid> InsertBotKeyAsync(BotKey key)
        {
            try
            {
                await _keys.InsertOneAsync(key);
                return key.Id;
            }
            catch (MongoDB.Driver.MongoWriteException e)
            {
                if (e.Message.Contains("E11000 duplicate key error collection"))
                {
                    throw new ConflictingBotKeyException("Botkey " + key.Id + " already exists.");
                }
                throw e;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        public async Task<bool> DeleteBotkey(Guid id)
        {
            var result = await _keys.DeleteOneAsync(u => u.Id == id);

            if (result.IsAcknowledged)
            {
                return true;
            }
            return false;
        }
    }
}
