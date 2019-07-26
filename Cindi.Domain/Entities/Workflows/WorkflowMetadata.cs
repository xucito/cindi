using ConsensusCore.Domain.BaseClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.Workflows
{
    public class WorkflowMetadata
    {
        public WorkflowMetadata()
        {
        }
        public Guid WorkflowId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Status { get; set; }
        public string WorkflowTemplateId { get; set; }
        public Dictionary<string, DateTime> LockedLogicBlocks = new Dictionary<string, DateTime>();
    }
}
