using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cindi.Presentation.ViewModels
{
    public class StepAssignmentVM
    {
        public Guid Id { get; set; }
        public string StepTemplateId { get; set; }
        public Dictionary<string, string> Inputs { get; set; }
    }
}
