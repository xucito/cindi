﻿using Cindi.Domain.Entities.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cindi.Domain.Utilities
{
    public static class WorkflowTemplateUtility
    {
        public static StepOutputReference GetHighestPriorityReference(StepOutputReference[] references, Step[] steps)
        {
            if (references == null)
            {
                return null;
            }

            var referenceCount = references.Count();
            if (referenceCount == 1)
            {
                return references.First();
            }
            else if (referenceCount == 0)
            {
                return null;
            }
            else
            {
                //All references with a existing step or is a reference to the workflow which is always there
                var filteredReferences = references.Where(r => steps.Where(s => s.Name == r.StepName).Count() > 0);

                foreach (var fr in filteredReferences)
                {
                    if (fr.Priority == null)
                    {
                        fr.Priority = -9999;
                    }
                }

                filteredReferences = filteredReferences.OrderBy(fr => fr.Priority);
                return filteredReferences.Last();
            }
        }
    }
}
