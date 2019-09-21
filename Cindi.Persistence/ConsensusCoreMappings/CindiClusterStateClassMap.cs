using Cindi.Domain.Entities.States;
using Cindi.Domain.ValueObjects;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.ConsensusCoreMappings
{
    public static class CindiClusterStateClassMap
    {
        public static void Register(BsonClassMap<CindiClusterState> cm)
        {
            cm.AutoMap();
            //cm.SetIsRootClass(true);
            cm.MapMember(s => s.LockedLogicBlocks).SetSerializer(new DictionaryInterfaceImplementerSerializer<Dictionary<string, LogicBlockLock>>(DictionaryRepresentation.ArrayOfArrays));
            /*cm.MapMember(s => s.Nodes).SetSerializer(new DictionaryInterfaceImplementerSerializer<ConcurrentDictionary<Guid, NodeInformation>>(DictionaryRepresentation.ArrayOfArrays));
            cm.MapMember(s => s.Indexes).SetSerializer(new DictionaryInterfaceImplementerSerializer<ConcurrentDictionary<string, Index>>(DictionaryRepresentation.ArrayOfArrays));
            cm.MapMember(s => s.ClusterTasks).SetSerializer(new DictionaryInterfaceImplementerSerializer<ConcurrentDictionary<Guid, BaseTask>>(DictionaryRepresentation.ArrayOfArrays));
    */   
    }
    }
}
