using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cindi.Presentation.ViewModels
{
    public class CreateExecutionScheduleVM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ExecutionTemplateName { get; set; }
        public string[] Schedule { get; set; }
        public bool EnableConcurrent { get; set; }
        public int TimeoutMs { get; set; }
    }
}
