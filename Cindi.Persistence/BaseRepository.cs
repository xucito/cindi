﻿using Cindi.Domain.Entities;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Sequences;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Entities.Users;
using Cindi.Persistence.GlobalValues;
using Cindi.Persistence.Journals;
using Cindi.Persistence.Sequences;
using Cindi.Persistence.Serializers;
using Cindi.Persistence.Steps;
using Cindi.Persistence.StepTemplates;
using Cindi.Persistence.TrackedEntities;
using Cindi.Persistence.Users;
using ConsensusCore.Domain.BaseClasses;
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
            BsonClassMap.RegisterClassMap<Sequence>(seq => SequencesClassMap.Register(seq));
            BsonClassMap.RegisterClassMap<StepTemplate>(cm => StepTemplatesClassMap.Register(cm));
            BsonClassMap.RegisterClassMap<Journal>(je => JournalsClassMap.Register(je));
            BsonClassMap.RegisterClassMap<Step>(cm => StepsClassMap.Register(cm));
            BsonClassMap.RegisterClassMap<StepLog>(cm => StepLogsClassMap.Register(cm));
            BsonClassMap.RegisterClassMap<User>(u => UsersClassMap.Register(u));
            BsonClassMap.RegisterClassMap<GlobalValue>(gv => GlobalValuesClassMap.Register(gv));
            BsonClassMap.RegisterClassMap<TrackedEntity>(gv => TrackedEntitiesClassMap.Register(gv));
            BsonSerializer.RegisterSerializer(typeof(BaseCommand), new BaseCommandSerializer());
        }
    }
}
