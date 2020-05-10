using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.ValueObjects;
using Cindi.Persistence.Serializers;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.ConsensusCoreMappings
{
    public static class BotKeyClassMap
    {
        public static void Register(BsonClassMap<BotKey> cm)
        {
            cm.AutoMap();
            cm.SetIsRootClass(true);
            cm.SetIgnoreExtraElements(true);
        }
    }
}
