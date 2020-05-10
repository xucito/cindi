using ConsensusCore.Domain.BaseClasses;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cindi.Persistence.ConsensusCoreMappings
{
    public static class BaseCommandsClassMap
    {
        public static void Register(BsonClassMap<BaseCommand> cm)
        {
            cm.AutoMap();
            AppDomain.CurrentDomain.GetAssemblies().SelectMany(assem => assem.GetTypes()).Where(type => type.IsSubclassOf(typeof(BaseCommand))).ToList()
                .ForEach(type => cm.AddKnownType(type));
            cm.SetIgnoreExtraElements(true);
        }
    }
}
