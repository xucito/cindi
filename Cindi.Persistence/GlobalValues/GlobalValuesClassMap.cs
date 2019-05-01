using Cindi.Domain.Entities.GlobalValues;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.GlobalValues
{
    public static class GlobalValuesClassMap
    {
        public static void Register(BsonClassMap<GlobalValue> sm)
        {
            sm.AutoMap();
            sm.MapIdMember(c => c.Id);
            sm.MapMember(m => m.Name).SetIsRequired(true);
            sm.SetIgnoreExtraElements(true);
        }
    }
}

