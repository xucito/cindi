using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.ExecutionTemplates
{
    public class ExecutionTemplate : TrackedEntity
    {
        public string Name { get; set; }
        public Dictionary<string, object> Inputs { get; set; }
        public string ReferenceId { get; set; }
        public string ExecutionTemplateType { get; set; }
        public bool IsDisabled { get; set; }
        public string Description { get; set; }

        public ExecutionTemplate()
        {
            ShardType = nameof(ExecutionTemplate);
        }

        public ExecutionTemplate(
            Guid id, 
            string name = "",
            string referenceId = "",
            string executionTemplateType = "",
            string description = "",
            string createdBy = "",
            Dictionary<string, object> inputs = null
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
                        FieldName = "inputs",
                        Value = inputs,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "referenceid",
                        Value = referenceId,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "executiontemplatetype",
                        Value = executionTemplateType,
                        Type = UpdateType.Create
                    },
                }
            }))
        {
            ShardType = nameof(ExecutionTemplate);
        }
    }
}
