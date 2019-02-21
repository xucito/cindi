using Cindi.Domain.Entities.Steps;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.StepTests
{
    public class TestBase
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public TestResult Evaluate(Step step) { return null; }
    }
}
