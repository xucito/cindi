using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cindi.Presentation.ViewModels
{
    public class UpdateExecutionScheduleVM
    {
        public bool? IsDisabled { get; set; }
        public string[] Schedule { get; set; }
        public string Description { get; set; }
    }
}
