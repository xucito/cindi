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
        public TemplateReference SequenceTemplateReference { get; set; }

        /// <summary>
        /// Input for the task, the Input name is the dictionary key and the input value is the Dictionary value
        /// </summary>
        public List<DynamicData> Inputs { get; set; }

        public Sequence(Guid Id, string Name, DateTime CreatedOn, TemplateReference templateReference)
        {
            this.Id = Id;
            this.Name = Name;
            this.CreatedOn = CreatedOn;
            this.SequenceTemplateReference = templateReference;
            Status = SequenceStatuses.Started;
        }
        
        public string Status { get; set; }
    }
}
