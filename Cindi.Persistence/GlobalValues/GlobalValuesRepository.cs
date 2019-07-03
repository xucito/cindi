using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Global;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Persistence.GlobalValues
{
    public class GlobalValuesRepository : BaseRepository, IGlobalValuesRepository
    {
        public IMongoCollection<GlobalValue> _globalValues;
        private IMongoCollection<JournalEntry> _journalEntries;

        public GlobalValuesRepository(string mongoDbConnectionString, string databaseName) : base(mongoDbConnectionString, databaseName)
        {
            var client = new MongoClient(mongoDbConnectionString);
            SetCollection(client);
        }

        public GlobalValuesRepository(IMongoClient client) : base(client)
        {
            SetCollection(client);
        }

        private void SetCollection(IMongoClient client)
        {
            var database = client.GetDatabase(DatabaseName);
            _globalValues = database.GetCollection<GlobalValue>("GlobalValues");
        }

        public async Task<GlobalValue> InsertGlobalValue(GlobalValue globalValue)
        {
            await _globalValues.InsertOneAsync(globalValue);
            return globalValue;
        }

        public async Task<GlobalValue> GetGlobalValueAsync(string globalValueName)
        {
            var globalValue = (await _globalValues.FindAsync(s => s.Name == globalValueName)).FirstOrDefault();
            return globalValue;
        }


        public async Task<List<GlobalValue>> GetGlobalValuesAsync(int size = 10, int page = 0, string sortOn = "name", string sortDirection = SortDirections.Ascending)
        {
            if (sortDirection != SortDirections.Ascending && sortDirection != SortDirections.Descending)
            {
                throw new InvalidValueException(sortDirection + " is not a valid sort direction.");
            }

            var sort = sortDirection == SortDirections.Ascending ? Builders<GlobalValue>.Sort.Ascending(sortOn) : Builders<GlobalValue>.Sort.Descending(sortOn);

            FindOptions<GlobalValue> options = new FindOptions<GlobalValue>
            {
                BatchSize = size,
                NoCursorTimeout = false,
                Skip = page,
                Limit = size,
                Sort = sort
            };

            var validGlobalValues = (await _globalValues.FindAsync(FilterDefinition<GlobalValue>.Empty, options)).ToList();

            return validGlobalValues;
        }

        public async Task<GlobalValue> GetGlobalValueAsync(Guid id)
        {
            var globalValue = (await _globalValues.FindAsync(s => s.Id == id)).FirstOrDefault();
            return globalValue;
        }
    }
}
