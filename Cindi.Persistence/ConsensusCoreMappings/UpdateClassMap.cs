using Cindi.Domain.ValueObjects;
using Cindi.Persistence.Serializers;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.ConsensusCoreMappings
{
    public static class UpdateClassMap
    {
        public static void Register(BsonClassMap<Update> cm)
        {
            cm.AutoMap();
        //   cm.GetMemberMap(c => c.Value).SetSerializer(new ObjectSerializer());
            cm.SetIsRootClass(true);
        }
    }
}
