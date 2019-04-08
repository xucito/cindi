using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.Sequences;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.Sequences
{
    public class SequenceStatuses
    {
        public static string Queued { get { return "queued"; } }
        public static string Started { get { return "started"; } }
        public static string Successful { get { return "successful"; } }
        public static string Warning { get { return "warning"; } }
        public static string Error { get { return "error"; } }
        public static string Unknown { get { return "unknown"; } }

        public static bool IsValid(string value)
        {
            if (value == Started ||
                value == Successful ||
                value == Warning ||
                value == Error || 
                value == Queued || 
                value == Unknown)
            {
                return true;
            }
            return false;
        }

        public static string ConvertStepStatusToSequenceStatus(string stepStatus)
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
            if (stepStatus == StepStatuses.Assigned || stepStatus == StepStatuses.Suspended)
            {
                return Started;
            }
            if (stepStatus == StepStatuses.Unassigned)
            {
                return Queued;
            }
            if (stepStatus == StepStatuses.Unknown)
            {
                return Unknown;
            }
            throw new InvalidSequenceStatusException(stepStatus + " is not a valid Step statuses.");
        }
    };
}
