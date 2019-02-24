using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.Steps
{
    public static class JournalEntriesMap
    {
        public static void Register(BsonClassMap<JournalEntry> cm)
        {
            cm.AutoMap();
            cm.SetIdMember(cm.GetMemberMap(c => c.Id));
            cm.SetIgnoreExtraElements(true);
        }
    }
}
