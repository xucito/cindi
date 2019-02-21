using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.Sequences
{
    public class SequenceStatuses
    {
        public static string Started { get { return "started"; } }
        public static string Successful { get { return "successful"; } }
        public static string Warning { get { return "warning"; } }
        public static string Error { get { return "error"; } }

        public static bool IsValid(string value)
        {
            if (value == Started ||
                value == Successful ||
                value == Warning ||
                value == Error)
            {
                return true;
            }
            return false;
        }
    };
}
