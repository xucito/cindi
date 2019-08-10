using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.WorkflowTemplates.ValueObjects
{
    public class SubsequentStep
    {
        public SubsequentStep()
        {
            Mappings = new List<Mapping>();
            IsPriority = false;
        }

        public string Description { get; set; }
        public string StepTemplateId { get; set; }
        public List<Mapping> Mappings { get; set; }
        public bool IsPriority { get; set; }
        public int StepRefId { get; set; }
    }
}
