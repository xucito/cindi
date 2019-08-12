using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.WorkflowTemplates.Conditions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Cindi.Domain.Tests.WorkflowTemplates.Conditions
{
    public class StepStatusCondition_Tests
    {
        public StepStatusCondition TestConditionIS = new StepStatusCondition()
        {
            Comparer = StepStatusConditionComparers.IS,
            Status = StepStatuses.Successful,
            WorkflowStepId = 0
        };

        public StepStatusCondition TestConditionISNOT = new StepStatusCondition()
        {
            Comparer = StepStatusConditionComparers.ISNOT,
            Status = StepStatuses.Successful,
            WorkflowStepId = 0
        };

        public StepStatusCondition TestConditionStatusCodeIS = new StepStatusCondition()
        {
            Comparer = StepStatusConditionComparers.IS,
            Status = StepStatuses.Successful,
            WorkflowStepId = 0,
            StatusCode = 1
        };

        public StepStatusCondition TestConditionStatusCodeISNOT = new StepStatusCondition()
        {
            Comparer = StepStatusConditionComparers.ISNOT,
            Status = StepStatuses.Successful,
            WorkflowStepId = 0,
            StatusCode = 1
        };

        [Fact]
        public void EvaluateMatchingStepStatus()
        {
            Assert.True(TestConditionIS.Evaluate(new List<Step>() {
                new Step()
                {
                    WorkflowStepId = 0,
                    Status = StepStatuses.Successful
                }
            }));

            Assert.False(TestConditionIS.Evaluate(new List<Step>() {
                new Step()
                {
                    WorkflowStepId = 0,
                    Status = StepStatuses.Suspended
                }
            }));

            Assert.False(TestConditionIS.Evaluate(new List<Step>() {
                new Step()
                {
                    WorkflowStepId = 0,
                    Status = StepStatuses.Warning
                }
            }));

            Assert.False(TestConditionISNOT.Evaluate(new List<Step>() {
                new Step()
                {
                    WorkflowStepId = 0,
                    Status = StepStatuses.Successful
                }
            }));

            Assert.True(TestConditionISNOT.Evaluate(new List<Step>() {
                new Step()
                {
                    WorkflowStepId = 0,
                    Status = StepStatuses.Suspended
                }
            }));

            Assert.True(TestConditionISNOT.Evaluate(new List<Step>() {
                new Step()
                {
                    WorkflowStepId = 0,
                    Status = StepStatuses.Warning
                }
            }));
        }

        [Fact]
        public void EvaluateMissingStepStatus()
        {
            Assert.False(TestConditionISNOT.Evaluate(new List<Step>() {
                new Step()
                {
                    WorkflowStepId = 1,
                    Status = StepStatuses.Warning
                }
            }));

            Assert.False(TestConditionISNOT.Evaluate(new List<Step>()
            {
            }));
        }

        [Fact]
        public void EvaluateMatchingStepStatusCode()
        {
            //Test that if the status is the same but status code is different return false
            Assert.False(TestConditionStatusCodeIS.Evaluate(new List<Step>() {
                new Step()
                {
                    WorkflowStepId = 0,
                    Status = StepStatuses.Successful,
                    StatusCode = 2
                }
            }));

            //Test if the status code is the same but status is different, step status will still return false
            Assert.False(TestConditionStatusCodeIS.Evaluate(new List<Step>() {
                new Step()
                {
                    WorkflowStepId = 0,
                    Status = StepStatuses.Warning,
                    StatusCode = 1
                }
            }));

            //Test if the step status and step code is the same return true
            Assert.True(TestConditionStatusCodeIS.Evaluate(new List<Step>() {
                new Step()
                {
                    WorkflowStepId = 0,
                    Status = StepStatuses.Successful,
                    StatusCode = 1
                }
            }));

            //Test that if the status is the same but status code is different return true for ISNOT
            Assert.True(TestConditionStatusCodeISNOT.Evaluate(new List<Step>() {
                new Step()
                {
                    WorkflowStepId = 0,
                    Status = StepStatuses.Successful,
                    StatusCode = 2
                }
            }));


            //Test that if the status is different but status code is the same return true for ISNOT
            Assert.True(TestConditionStatusCodeISNOT.Evaluate(new List<Step>() {
                new Step()
                {
                    WorkflowStepId = 0,
                    Status = StepStatuses.Warning,
                    StatusCode = 1
                }
            }));
        }
    }
}

