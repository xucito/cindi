using ConsensusCore.Domain.BaseClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.ClusterRPC
{
    public class UpdateClusterDetails : BaseCommand
    {
        public string Id { get; set; }
        public bool? AssignmentEnabled { get; set; } = null;
        public string Version { get; set; }
        public string EncryptionKeyHash { get; set; }
        public byte[] EncryptionKeySalt { get; set; } = null;
        public bool? AllowAutoRegistration { get; set; } = null;
        public bool? Initialized { get; set; } = null;
        public string MetricRetentionPeriod { get; set; } = null;
        public bool DefaultIfNull { get; set; } = false;
        public string StepRetentionPeriod { get; set; } = null;

        public override string CommandName => "UpdateClusterDetails";
    }
}
