using Cindi.Domain.Entities;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Entities.Users;
using Cindi.Domain.ValueObjects;
using Cindi.Persistence.GlobalValues;
using Cindi.Persistence.Journals;
using Cindi.Persistence.Workflows;
using Cindi.Persistence.Serializers;
using Cindi.Persistence.Steps;
using Cindi.Persistence.StepTemplates;
using Cindi.Persistence.TrackedEntities;
using Cindi.Persistence.Users;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.Services;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using Cindi.Domain.Entities.WorkflowTemplates.Conditions;
using ConditionSerializer = Cindi.Persistence.Serializers.ConditionSerializer;
using System.Linq;
using Cindi.Persistence.WorkflowTemplates.Conditions;
using Cindi.Persistence.ConsensusCoreMappings;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using Cindi.Domain.Entities.States;
using ConsensusCore.Domain.Models;

namespace Cindi.Persistence
{
    public abstract class BaseRepository
    {
        public string DatabaseName { get; } = "CindiDb";

        public BaseRepository(string connectionString, string databaseName)
        {
            DatabaseName = databaseName;
        }

        public BaseRepository(IMongoClient client) { }

        public static void RegisterClassMaps()
        {
            BsonClassMap.RegisterClassMap<WorkflowMetadata>(cm => WorkflowsClassMap.Register(cm));
            BsonClassMap.RegisterClassMap<Workflow>(seq => WorkflowsClassMap.Register(seq));
            BsonClassMap.RegisterClassMap<StepTemplate>(cm => StepTemplatesClassMap.Register(cm));
            BsonClassMap.RegisterClassMap<Journal>(je => JournalsClassMap.Register(je));
            BsonClassMap.RegisterClassMap<Step>(cm => StepsClassMap.Register(cm));
            BsonClassMap.RegisterClassMap<StepLog>(cm => StepLogsClassMap.Register(cm));
            BsonClassMap.RegisterClassMap<User>(u => UsersClassMap.Register(u));
            BsonClassMap.RegisterClassMap<GlobalValue>(gv => GlobalValuesClassMap.Register(gv));
            BsonClassMap.RegisterClassMap<TrackedEntity>(gv => TrackedEntitiesClassMap.Register(gv));
            BsonClassMap.RegisterClassMap<Condition>(gv => ConditionsClassMap.Register(gv));
            BsonClassMap.RegisterClassMap<LogEntry>();
            BsonClassMap.RegisterClassMap<NodeInformation>();
            BsonClassMap.RegisterClassMap<Index>();
            BsonClassMap.RegisterClassMap<BaseTask>(); 
            BsonClassMap.RegisterClassMap<ObjectLock>();
            BsonClassMap.RegisterClassMap<LogicBlockLock>();
            BsonClassMap.RegisterClassMap<BaseCommand>(gv => BaseCommandsClassMap.Register(gv));
            BsonClassMap.RegisterClassMap<NodeStorage<CindiClusterState>>(gv => NodeStorageClassMap.Register(gv));
            BsonClassMap.RegisterClassMap<Update>(gv => UpdateClassMap.Register(gv));
            BsonClassMap.RegisterClassMap<LocalShardMetaData>(lsm => LocalShardMetaDataClassMap.Register(lsm));
            BsonClassMap.RegisterClassMap<CindiClusterState>(cs => CindiClusterStateClassMap.Register(cs));
            BsonClassMap.RegisterClassMap<ShardOperation>(cs => ShardOperationClassMap.Register(cs));
            BsonClassMap.RegisterClassMap<BaseState>(cs => BaseStateClassMap.Register(cs));
            /*BsonSerializer.RegisterSerializer(typeof(BaseCommand), new BaseCommandSerializer());
            BsonSerializer.RegisterSerializer(typeof(NodeStorage), new NodeStorageSerializer());
            BsonSerializer.RegisterSerializer(typeof(Update), new UpdateSerializer());*/
        }
    }
}
