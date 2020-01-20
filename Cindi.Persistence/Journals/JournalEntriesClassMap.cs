using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.ValueObjects;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.Journals
{
    public static class JournalEntriesClassMap
    {
        public static void Register(BsonClassMap<JournalEntry> journal)
        {
            journal.AutoMap();
           // journal.MapMember(c => c.Updates).SetSerializer(new EnumerableInterfaceImplementerSerializer<List<Update>>());
            journal.SetIgnoreExtraElements(true);
            //cm.SetIdMember(cm.GetMemberMap(c => c.Id)).SetIdGenerator(CombGuidGenerator.Instance); 
        }
    }
}
