using Cindi.Domain.Entities;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.TrackedEntities
{
    public static class TrackedEntitiesClassMap
    {
        public static void Register(BsonClassMap<TrackedEntity> cm)
        {
            cm.AutoMap();
        }
    }
}
