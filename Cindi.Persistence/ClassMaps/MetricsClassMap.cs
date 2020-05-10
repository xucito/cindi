using Cindi.Domain.Entities.Metrics;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.Metrics
{
    public static class MetricsClassMap
    {
        public static void Register(BsonClassMap<Metric> cm)
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
        }
    }
}
