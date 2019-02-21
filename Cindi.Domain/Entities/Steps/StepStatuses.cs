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
        public const string Warning = "warning";
        public const string Error = "error";

        public static string[] AllStatuses = new string[]{
            Suspended,
            Unassigned,
            Assigned,
            Successful,
            Warning,
            Error
        };

        public static bool IsValid(string value)
        {
            if (value == Unassigned ||
                value == Assigned ||
                value == Successful ||
                value == Warning ||
                value == Error ||
                value == Suspended)
            {
                return true;
            }
            return false;
        }
    };
}
