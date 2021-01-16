using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.ClusterRPC
{
    public class UpdateClusterDetails
    {
        public bool? AllowAutoRegistration { get; set; } = null;
        public bool? AssignmentEnabled { get; set; } = true;
        public string MetricRetentionPeriod { get; set; } = null;
        public string StepRetentionPeriod { get; set; } = null;
        public int? CleanupInterval { get; set; } = null;
    }
}
