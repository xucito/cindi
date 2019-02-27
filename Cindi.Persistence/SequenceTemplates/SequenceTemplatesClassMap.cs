using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.SequencesTemplates;
using Cindi.Domain.Entities.Steps;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.SequenceTemplates
{
    public static class SequenceTemplatesEntriesMap
    {
        public static void Register(BsonClassMap<SequenceTemplate> cm)
        {
            cm.AutoMap();
            cm.SetIdMember(cm.GetMemberMap(c => c.Id));
            cm.SetIgnoreExtraElements(true);
        }
    }
}
