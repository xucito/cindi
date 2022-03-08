using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cindi.Presentation.ViewModels
{
    public class PutStepStatus
    {
        public string Status { get; set; }
        public DateTimeOffset? SuspendedUntil { get; set; }
    }
}
