using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.WorkflowTemplates.ValueObjects
{
    public class SubsequentStep
    {
        public SubsequentStep()
        {
            Mappings = new Dictionary<string, Mapping>();
            IsPriority = false;
        }

        public string Description { get; set; }
        public string StepTemplateId { get; set; }
        /// <summary>
        /// the dictionary key is the field to map to
        /// </summary>
        public Dictionary<string, Mapping> Mappings { get; set; }
        public bool IsPriority { get; set; }
    }
}
