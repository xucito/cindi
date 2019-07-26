using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.Workflows;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Cindi.Domain.Entities.Workflows
{
    /// <summary>
    /// Immutable reference to a workflow
    /// </summary>
    public class Workflow : TrackedEntity
    {
        public Workflow()
        {
            ShardType = nameof(Workflow);
        }
        public Workflow(
            Guid id,
            string workflowTemplateId,
            Dictionary<string, object> inputs,
            string name,
            string createdBy,
            DateTime createdOn
            ) : base(
            new Journal(new JournalEntry()
            {
                Updates = new List<Update>()
                {
                    new Update()
                    {
                        FieldName = "id",
                        Value = id,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "workflowtemplateid",
                        Value = workflowTemplateId,
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
                        FieldName = "name",
                        Value = name,
                        Type = UpdateType.Create
                    },
                    new Update()
                    {
                        FieldName = "createdon",
                        Value = createdOn,
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
                        FieldName = "status",
                        Value = WorkflowStatuses.Started,
                        Type = UpdateType.Create
                    }
                }
            })
            )
        {
            ShardType = nameof(Workflow);
        }

        public string Name { get; set; }
        public string WorkflowTemplateId { get; set; }

        /// <summary>
        /// Input for the task, the Input name is the dictionary key and the input value is the Dictionary value
        /// </summary>
        public Dictionary<string, object> Inputs { get; set; }

        public string Status { get; set; }

        public WorkflowMetadata Metadata
        {
            get
            {
                return
                new WorkflowMetadata()
                {
                    WorkflowId = Id,
                    CreatedOn = CreatedOn,
                    WorkflowTemplateId = WorkflowTemplateId,
                    Status = Status
                };
            }
        }
    }
}
