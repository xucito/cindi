using Cindi.Domain.Entities.Steps;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.StepTests
{
    public class StatusTest : TestBase
    {
        public List<string> ValidStatuses = new List<string>();
        public List<string> InvalidStatuses = new List<string>();

        public TestResult Evaluate(Step step)
        {
            if (InvalidStatuses != null)
            {
                foreach (var status in InvalidStatuses)
                {
                    if (step.Status.ToLower() == status.ToLower())
                    {
                        return new TestResult()
                        {
                            IsPassing = false,
                            Id = Id,
                            Message = "Test unsuccessful, step's status " + step.Status + " matches invalid status " + status
                        };
                    }
                }
            }

            if (ValidStatuses != null)
            {
                foreach (var status in ValidStatuses)
                {
                    if (step.Status.ToLower() == status.ToLower())
                    {
                        return new TestResult()
                        {
                            IsPassing = true,
                            Id = Id,
                            Message = "Test Successful, step's status " + step.Status + " matches valid status " + status
                        };
                    }
                }

                return new TestResult()
                {
                    IsPassing = false,
                    Id = Id,
                    Message = "No status conditions met."
                };
            }

            return new TestResult()
            {
                IsPassing = true,
                Id = Id,
                Message = "No status conditions met, defaulting to pass."
            };
        }
    }
}
