﻿using Cindi.Domain.Entities.Steps;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.StepTests
{
    public class StepTestTemplate
    {
        /// <summary>
        /// Name of definition
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Version of the definition
        /// </summary>
        public string Version { get; set; }

        public string Description { get; set; }

        public TemplateReference Reference
        {
            get
            {
                return new TemplateReference()
                {
                    Name = this.Name,
                    Version = this.Version
                };
            }
        }

        //public TemplateReference StepTemplate { get; set; }
        public Dictionary<string, string> Inputs { get; set; }
        public List<TestBase> Tests { get; set; }
        public DateTime CreatedOn { get; set; }

        public StepTestTemplate()
        {
            Tests = new List<TestBase>();
            Inputs = new Dictionary<string, string>();
        }

        public StepTestResult EvaluateStep(Step step)
        {
            var startTime = DateTime.Now;
            List<TestResult> results = new List<TestResult>();
           
            foreach(var test in Tests)
            {
                results.Add(test.Evaluate(step));
            }

            return new StepTestResult() {
                StepTestId = this.Reference,
                TestResults = results,
                StartTime = startTime,
                CompletionTime = DateTime.Now
            };
        }

        public bool IsPassing(Step step)
        {
            List<TestResult> results = new List<TestResult>();

            foreach (var test in Tests)
            {
                if(!test.Evaluate(step).IsPassing)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
