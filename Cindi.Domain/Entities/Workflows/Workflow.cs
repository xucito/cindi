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

        /// <summary>
        /// Logic blocks that no longer need to be evaluated
        /// </summary>
        public List<string> CompletedLogicBlocks { get; set; } = new List<string>();
        public Guid? ExecutionTemplateId { get; set; }
        public Guid? ExecutionScheduleId { get; set; }
    }
}
