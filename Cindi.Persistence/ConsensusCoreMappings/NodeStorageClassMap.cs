using Cindi.Domain.Entities.States;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.Models;
using ConsensusCore.Domain.Services;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cindi.Persistence.ConsensusCoreMappings
{
    public static class NodeStorageClassMap
    {
        public static void Register(BsonClassMap<NodeStorage<CindiClusterState>> cm)
        {
            cm.AutoMap();
            cm.SetIsRootClass(true);
            cm.UnmapMember(c => c._saveThread);
            cm.MapMember(s => s.Logs).SetSerializer(new DictionaryInterfaceImplementerSerializer<SortedList<int, LogEntry>>(DictionaryRepresentation.ArrayOfArrays));
            /*AppDomain.CurrentDomain.GetAssemblies().SelectMany(assem => assem.GetTypes()).Where(type => type.IsSubclassOf(typeof(NodeStorage))).ToList()
                .ForEach(type => cm.AddKnownType(type));*/
        }
    }
}
