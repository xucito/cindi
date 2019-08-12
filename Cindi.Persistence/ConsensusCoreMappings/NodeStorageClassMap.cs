using ConsensusCore.Domain.Services;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cindi.Persistence.ConsensusCoreMappings
{
    public static class NodeStorageClassMap
    {
        public static void Register(BsonClassMap<NodeStorage> cm)
        {
            cm.AutoMap();
            cm.SetIsRootClass(true);
            cm.UnmapMember(c => c._saveThread);
            var featureType = typeof(NodeStorage);
            AppDomain.CurrentDomain.GetAssemblies().SelectMany(assem => assem.GetTypes()).Where(type => type.IsSubclassOf(typeof(NodeStorage))).ToList()
                .ForEach(type => cm.AddKnownType(type));
        }
    }
}
