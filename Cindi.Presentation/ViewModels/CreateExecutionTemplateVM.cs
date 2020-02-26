using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cindi.Presentation.ViewModels
{
    public class CreateExecutionTemplateVM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<string, object> Inputs { get; set; }
        public string ExecutionTemplateType { get; set; }
        public string ReferenceId { get; set; }
        public string CreatedBy { get; set; }
    }
}
