using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.ExecutionSchedule
{
    public class ExecutionSchedule : TrackedEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ExecutionTemplateName {get;set;}
        public bool IsDisabled { get; set; }
        public DateTime NextRun { get; set; }
        /// <summary>
        /// Cron based scheduling
        /// If multiple schedules are given, the next valid time is used
        /// </summary>
        public string[] Schedule { get; set; }

        public ExecutionSchedule()
        {
            ShardType = nameof(ExecutionSchedule);
        }

        public ExecutionSchedule(
            Guid id,
            string name = "",
            string executionTemplateName = "",
            string description = "",
            string createdBy = "",
            string[] schedule = null,
            DateTime? nextRun = null
            ) : base(
            new Journal(new JournalEntry()
            {
                Updates = new List<Update>()
                {
                    new Update()
                    {
                        FieldName = "name",
                        Value = name,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "description",
                        Value = description,
                        Type = UpdateType.Create
                    },
                   new Update()
                    {
                        FieldName = "createdby",
                        Value = createdBy,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "id",
                        Value = id,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "createdon",
                        Value = DateTime.UtcNow,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "executiontemplatename",
                        Value = executionTemplateName,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "schedule",
                        Value = schedule,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "nextrun",
                        Value = nextRun.Value,
                        Type = UpdateType.Create
                    },
                }
            }))
        {
            ShardType = nameof(ExecutionSchedule);
        }
    }
}
