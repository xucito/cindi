using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.Metrics;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Entities.Users;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Entities.WorkflowsTemplates;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Persistence
{
    public static class EntitiesTableMap
    {
        private static Dictionary<Type, string> Table = new Dictionary<Type, string>()
        {
            { typeof(Step), "Steps" },
            { typeof(Workflow), "Workflows" },
            { typeof(User), "Users" },
            { typeof(BotKey), "BotKeys" },
            { typeof(GlobalValue), "GlobalValues" },
            { typeof(StepTemplate), "StepTemplates" },
            { typeof(WorkflowTemplate), "WorkflowTemplates" },
            { typeof(Metric), "Metrics" },
            { typeof(MetricTick), "MetricTicks" }
        };

        public static Dictionary<Type, string> Table1 { get => Table; set => Table = value; }

        public static string GetTableName<T>()
        {
            return Table1[typeof(T)];
        }
    }
}
