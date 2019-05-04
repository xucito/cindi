using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.Journals
{
    public static class JournalsClassMap
    {
        public static void Register(BsonClassMap<Journal> journal)
        {
            journal.AutoMap();
            journal.MapMember(c => c.JournalEntries).SetSerializer(new DictionaryInterfaceImplementerSerializer<SortedDictionary<int, JournalEntry>>(DictionaryRepresentation.ArrayOfDocuments));
            journal.SetIgnoreExtraElements(true);
            //cm.SetIdMember(cm.GetMemberMap(c => c.Id)).SetIdGenerator(CombGuidGenerator.Instance); 
        }
    }
}
