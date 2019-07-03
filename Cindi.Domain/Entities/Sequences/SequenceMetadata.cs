using ConsensusCore.Domain.BaseClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.Sequences
{
    public class SequenceMetadata
    {
        public SequenceMetadata()
        {
        }
        public Guid SequenceId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Status { get; set; }
        public string SequenceTemplateId { get; set; }
        public Dictionary<string, DateTime> LockedLogicBlocks = new Dictionary<string, DateTime>();
    }
}
