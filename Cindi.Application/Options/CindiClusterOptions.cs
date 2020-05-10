using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Options
{
    public class CindiClusterOptions
    {
        /// <summary>
        /// The default time for which steps should be suspended for
        /// </summary>
        public double DefaultSuspensionTimeMs { get; set; } = 1000;
        /// <summary>
        /// MongoDB Connection String
        /// </summary>
        public string DbConnectionString { get; set; }
        /// <summary>
        /// Enable this node to serve clients assets
        /// </summary>
        public bool EnableUI { get; set; } = false;
        /// <summary>
        /// Enable monitoring on the node
        /// </summary>
        public bool EnableMonitoring { get; set; } = true;
    }
}
