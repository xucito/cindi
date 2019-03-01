using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cindi.Presentation.ViewModels
{
    public class CompleteStepVM
    {
        public Dictionary<string, object> Outputs { get; set; }
        public string Status { get; set; }
        public int StatusCode { get; set; }
        public string Logs { get; set; }
    }
}
