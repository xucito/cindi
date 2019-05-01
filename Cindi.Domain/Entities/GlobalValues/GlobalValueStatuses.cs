using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.GlobalValues
{
    public static class GlobalValueStatuses
    {
        public static string Enabled { get { return "enabled"; } }
        public static string Disabled { get { return "disabled"; } }
        /// <summary>
        /// A global value with this status is unable to be referenced and will error any steps with this value
        /// </summary>
        public static string Blocked { get { return "blocked"; } }
    }
}
