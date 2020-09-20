using Cindi.Domain.Entities.Workflows;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence.Workflows
{
    public static class WorkflowsClassMap
    {
        public static void Register(BsonClassMap<WorkflowMetadata> cm)
        {
            cm.AutoMap();
            cm.MapIdMember(s => s.WorkflowId);
            cm.SetIgnoreExtraElements(true);
        }

        public static void Register(BsonClassMap<Workflow> sm)
        {
            sm.AutoMap();
        }
    }
}
