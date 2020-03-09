using Cindi.Domain.Entities.ExecutionSchedule;
using Cindi.Domain.Entities.ExecutionTemplates;
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
    public static class ExecutionScheduleClassMap
    {
        public static void Register(BsonClassMap<ExecutionSchedule> cm)
        {
            cm.AutoMap();
        }
    }
}
