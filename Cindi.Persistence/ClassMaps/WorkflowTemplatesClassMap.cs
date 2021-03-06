﻿using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Domain.Entities.Steps;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using Cindi.Domain.Entities.WorkflowTemplates.Conditions;
using System.Linq;

namespace Cindi.Persistence.WorkflowTemplates
{
    public static class WorkflowTemplatesClassMap
    {
        public static void Register(BsonClassMap<WorkflowTemplate> cm)
        {
            cm.AutoMap();
            cm.SetIdMember(cm.GetMemberMap(c => c.Id));
            cm.SetIgnoreExtraElements(true);
        }
    }
}
