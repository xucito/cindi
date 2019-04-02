using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace Cindi.Application.Services.ClusterState
{
    public class ClusterState
    {
        public const string DefaultId = "State";
        public string Id { get; set; } = DefaultId;
        public Dictionary<string, DateTime> LockedLogicBlocks = new Dictionary<string, DateTime>();
        public bool AssignmentEnabled { get; set; } = true;
        public string Version { get; set; } = "1.0";
        
        public string EncryptionKeyHash { get; set; }
        public byte[] EncryptionKeySalt { get; set; }

        public bool AllowAutoRegistration { get; set; }
        public bool Initialized { get; set; } = false;
    }
}
