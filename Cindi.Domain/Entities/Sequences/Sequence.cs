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
    public class Sequence
    {
        public Guid Id { get; }
        public string Name { get; }
        public DateTime CreatedOn { get; }
        public string SequenceTemplateId { get; set; }

        /// <summary>
        /// Input for the task, the Input name is the dictionary key and the input value is the Dictionary value
        /// </summary>
        public List<DynamicData> Inputs { get; set; }

        public string Status { get; set; }
    }
}
