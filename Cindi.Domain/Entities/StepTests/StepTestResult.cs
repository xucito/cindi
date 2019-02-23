using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.StepTests
{
    public class StepTestResult
    {
        public TemplateReference StepTestId { get; set; }
        public List<TestResult> TestResults { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? CompletionTime { get; set; }
    }
}
