using Cindi.Domain.Entities.Steps;
using MongoDB.Bson.Serialization;
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
            cm.SetIdMember(cm.GetMemberMap(c => c.Id));
            cm.SetIgnoreExtraElements(true);
            cm.UnmapMember(c => c.Status);
            cm.UnmapMember(c => c.IsComplete);
            cm.UnmapMember(c => c.Outputs);
            cm.UnmapMember(c => c.TestResults);
            cm.UnmapMember(c => c.Journal);
        }
    }
}
