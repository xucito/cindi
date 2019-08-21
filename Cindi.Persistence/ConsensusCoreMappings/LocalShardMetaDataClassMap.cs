using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.Services;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.ConsensusCoreMappings
{
     public static class LocalShardMetaDataClassMap
    {
        public static void Register(BsonClassMap<LocalShardMetaData> cm)
        {
            cm.AutoMap();
            cm.MapMember(s => s.ShardOperations).SetSerializer(new DictionaryInterfaceImplementerSerializer<ConcurrentDictionary<int, ShardOperation>>(DictionaryRepresentation.ArrayOfArrays));
            cm.MapMember(s => s.ObjectsMarkedForDeletion).SetSerializer(new DictionaryInterfaceImplementerSerializer<ConcurrentDictionary<Guid, DateTime>>(DictionaryRepresentation.ArrayOfArrays));
        }
    }
}
