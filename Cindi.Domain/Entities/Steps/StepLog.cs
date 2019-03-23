using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.Steps
{
    public class StepLog: TrackedEntity
    {
        public string Message { get; set; }
    }
}
