using Cindi.Domain.Entities.StepTemplates;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.StepTemplates
{
    public static class StepTemplatesClassMap
    {
        public static void Register(BsonClassMap<StepTemplate> cm)
        {
            cm.AutoMap();
            cm.SetIdMember(cm.GetMemberMap(c => c.Id));
            cm.SetIgnoreExtraElements(true);
        }
    }
}
