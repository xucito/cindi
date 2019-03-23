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
    public class Sequence: TrackedEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SequenceTemplateId { get; set; }

        /// <summary>
        /// Input for the task, the Input name is the dictionary key and the input value is the Dictionary value
        /// </summary>
        public Dictionary<string, object> Inputs { get; set; }

        public List<Step> Steps { get; set; }

        public Journal Journal { get; set; }

        public string Status
        {
            get
            {
                var status = Journal.GetLatestValueOrDefault<string>("status", null);
                if (status == null)
                {
                    throw new InvalidSequenceStatusException("Status for sequence " + Id + " was not found.");
                }
                return status;
            }
        }

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
