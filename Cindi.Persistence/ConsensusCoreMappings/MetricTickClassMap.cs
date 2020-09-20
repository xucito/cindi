using Cindi.Domain.Entities.Metrics;
using Cindi.Domain.ValueObjects;
using Cindi.Persistence.Serializers;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.ConsensusCoreMappings
{
    public static class MetricTickClassMap
    {
        public static void Register(BsonClassMap<MetricTick> cm)
        {
            cm.AutoMap();
            cm.SetIsRootClass(true);
            cm.SetIgnoreExtraElements(true);
        }
    }
}
