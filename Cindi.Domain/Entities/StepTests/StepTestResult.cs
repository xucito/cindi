using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.StepTests
{
    public class StepTestResult
    {
        public string StepTestTemplateId { get; set; }
        public List<TestResult> TestResults { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? CompletionTime { get; set; }
    }
}
