using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Sequences;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Entities.Users;
using Cindi.Persistence.Sequences;
using Cindi.Persistence.Steps;
using Cindi.Persistence.StepTemplates;
using Cindi.Persistence.Users;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence
{
    public abstract class BaseRepository
    {
        public string DatabaseName { get; } = "CindiDb";

        public BaseRepository(string connectionString, string databaseName) {
            DatabaseName = databaseName;
        }

        public BaseRepository(IMongoClient client) { }

        public static void RegisterClassMaps()
        {
            BsonClassMap.RegisterClassMap<SequenceMetadata>(cm => SequencesClassMap.Register(cm));
            BsonClassMap.RegisterClassMap<StepMetadata>(cm => StepsClassMap.Register(cm));
            BsonClassMap.RegisterClassMap<StepTemplate>(cm => StepTemplatesClassMap.Register(cm));
            BsonClassMap.RegisterClassMap<JournalEntry>(je => JournalEntriesClassMap.Register(je));
            BsonClassMap.RegisterClassMap<Step>(cm => StepsClassMap.Register(cm));
            BsonClassMap.RegisterClassMap<User>(u => UsersClassMap.Register(u));
        }
    }
}
