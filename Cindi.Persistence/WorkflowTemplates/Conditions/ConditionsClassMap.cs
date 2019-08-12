using Cindi.Domain.Entities.WorkflowTemplates.Conditions;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cindi.Persistence.WorkflowTemplates.Conditions
{
    public static class ConditionsClassMap
    {
        public static void Register(BsonClassMap<Condition> cm)
        {
            cm.AutoMap();
            cm.SetIsRootClass(true);
            var featureType = typeof(Condition);
            featureType.Assembly.GetTypes()
                .Where(type => featureType.IsAssignableFrom(type)).ToList()
                .ForEach(type => cm.AddKnownType(type));
        }
    }
}
