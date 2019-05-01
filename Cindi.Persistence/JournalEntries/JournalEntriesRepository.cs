using Cindi.Domain.Entities.JournalEntries;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.JournalEntries
{
    public class JournalEntriesRepository : BaseRepository
    {
        private IMongoCollection<JournalEntry> _journalEntries;

        public JournalEntriesRepository(string mongoDbConnectionString, string databaseName) : base(mongoDbConnectionString, databaseName)
        {
            var client = new MongoClient(mongoDbConnectionString);
            SetCollection(client);
        }

        public JournalEntriesRepository(IMongoClient client) : base(client)
        {
            SetCollection(client);
        }

        private void SetCollection(IMongoClient client)
        {
            var database = client.GetDatabase(DatabaseName);
            _journalEntries = database.GetCollection<JournalEntry>("JournalEntries");
        }
    }
}
