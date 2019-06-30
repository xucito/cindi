using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.Steps
{
    public static class StepsClassMap
    {
        public static void Register(BsonClassMap<Step> cm)
        {
            cm.AutoMap();
          //  cm.MapIdMember(c => c.Id);
            //cm.SetIdMember(cm.GetMemberMap(c => c.Id));
            //cm.SetIgnoreExtraElements(true);
            /*cm.UnmapMember(c => c.Status);
            cm.UnmapMember(c => c.IsComplete);
            cm.UnmapMember(c => c.Outputs);
            cm.UnmapMember(c => c.TestResults);
            cm.UnmapMember(c => c.Journal);*/
        }
    }

    public static class StepLogsClassMap
    {
        public static void Register(BsonClassMap<StepLog> cm)
        {
            cm.AutoMap();
        }
    }
}
