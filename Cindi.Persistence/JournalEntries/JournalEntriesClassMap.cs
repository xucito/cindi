using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.Steps
{
    public static class JournalEntriesClassMap
    {
        public static void Register(BsonClassMap<JournalEntry> cm)
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
            //cm.SetIdMember(cm.GetMemberMap(c => c.Id)).SetIdGenerator(CombGuidGenerator.Instance); 
        }
    }
}
