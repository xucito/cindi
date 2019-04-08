using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.StepTests
{
    public class TestResult
    { 
        public bool IsPassing { get; set; }
        public int Id { get; set; }
        public string Message { get; set; }
    }
}
