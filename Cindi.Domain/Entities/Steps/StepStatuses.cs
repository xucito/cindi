using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.Steps
{
    /// <summary>
    /// The status of the task
    /// </summary>
    public static class StepStatuses
    {
        public const string Suspended = "suspended";
        public const string Unassigned = "unassigned";
        public const string Assigned = "assigned";
        public const string Successful = "successful";
        public const string Cancelled = "cancelled";
        public const string Warning = "warning";
        public const string Error = "error";
        public const string Unknown = "unknown";

        private static Dictionary<string, int> _priorityDictionary = new Dictionary<string, int>()
        {
            { Cancelled, 1 },
            {Error, 2 },
            {Warning,3 },
            {Successful,4 },
            {Unknown,5 },
            { Assigned,6 },
            {Suspended, 7 },
            {Unassigned,8},
        };

        public static string[] AllStatuses = new string[]{
            Cancelled,
            Suspended,
            Unassigned,
            Assigned,
            Successful,
            Warning,
            Error,
            Unknown
        };

        public static bool IsCompleteStatus(string status)
        {
            if (status == Successful ||
                status == Warning ||
                status == Error || 
                status == Cancelled)
            {
                return true;
            }
            return false;
        }

        public static string GetHighestPriority(string[] statuses)
        {
            string highestPriority = Unknown;
            foreach (var status in statuses)
            {
                if (_priorityDictionary[status] < _priorityDictionary[highestPriority])
                {
                    highestPriority = status;
                }
            }
            return highestPriority;
        }

        public static bool IsValid(string value)
        {
            if (value == Unassigned ||
                value == Assigned ||
                value == Successful ||
                value == Warning ||
                value == Error ||
                value == Suspended ||
                value == Unknown ||
                value == Cancelled 
                )
            {
                return true;
            }
            return false;
        }
    };
}
