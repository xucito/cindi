using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Services.ClusterState
{
    public class ClusterSettings
    {
        public bool AllowAutoRegistration { get; set; } = true;
        public bool AssignmentEnabled { get; set; } = true;
        /// <summary>
        /// Uses date time maths
        /// </summary>
        public string MetricRetentionPeriod { get; set; } = "24h";
    }
}
