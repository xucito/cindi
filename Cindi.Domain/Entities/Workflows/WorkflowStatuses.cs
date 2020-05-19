using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.Workflows;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.Workflows
{
    public class WorkflowStatuses
    {
       // public static string Queued { get { return "queued"; } }
        public static string Started { get { return "started"; } }
        public static string Successful { get { return "successful"; } }
        public static string Warning { get { return "warning"; } }
        public static string Error { get { return "error"; } }
        public static string Unknown { get { return "unknown"; } }
        public static string Cancelled { get { return "cancelled"; } }

        public static bool IsValid(string value)
        {
            if (value == Started ||
                value == Successful ||
                value == Warning ||
                value == Error || 
                //value == Queued || 
                value == Unknown ||
                value == Cancelled)
            {
                return true;
            }
            return false;
        }

        public static string[] CompletedStatus { get {
                return new string[] {
            Successful,
            Warning,
            Error,
            Cancelled
        };
            } }

        public static string ConvertStepStatusToWorkflowStatus(string stepStatus)
        {
            if(stepStatus == StepStatuses.Error)
            {
                return Error;
            }
            if (stepStatus == StepStatuses.Warning)
            {
                return Warning;
            }
            if (stepStatus == StepStatuses.Successful)
            {
                return Successful;
            }
            if (stepStatus == StepStatuses.Assigned || stepStatus == StepStatuses.Suspended || stepStatus == StepStatuses.Unassigned)
            {
                return Started;
            }
            if (stepStatus == StepStatuses.Unknown)
            {
                return Unknown;
            }
            if(stepStatus == StepStatuses.Cancelled)
            {
                return Cancelled;
            }
            throw new InvalidWorkflowStatusException(stepStatus + " is not a valid Step statuses.");
        }
    };
}
