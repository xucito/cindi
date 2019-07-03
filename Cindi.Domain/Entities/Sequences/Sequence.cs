using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.Sequences;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Cindi.Domain.Entities.Sequences
{
    /// <summary>
    /// Immutable reference to a sequence
    /// </summary>
    public class Sequence : TrackedEntity
    {
        public Sequence()
        {
            ShardType = nameof(Sequence);
        }
        public Sequence(
            Guid id,
            string sequenceTemplateId,
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
                        FieldName = "sequencetemplateid",
                        Value = sequenceTemplateId,
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
                        Value = SequenceStatuses.Started,
                        Type = UpdateType.Create
                    }
                }
            })
            )
        {
            ShardType = nameof(Sequence);
        }

        public string Name { get; set; }
        public string SequenceTemplateId { get; set; }

        /// <summary>
        /// Input for the task, the Input name is the dictionary key and the input value is the Dictionary value
        /// </summary>
        public Dictionary<string, object> Inputs { get; set; }

        public string Status { get; set; }

        public SequenceMetadata Metadata
        {
            get
            {
                return
                new SequenceMetadata()
                {
                    SequenceId = Id,
                    CreatedOn = CreatedOn,
                    SequenceTemplateId = SequenceTemplateId,
                    Status = Status
                };
            }
        }
    }
}
