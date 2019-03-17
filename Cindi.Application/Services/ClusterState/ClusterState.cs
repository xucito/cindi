using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Services.ClusterState
{
    public class ClusterState
    {
        public const string DefaultId = "State";
        public string Id { get; } = DefaultId;
        public Dictionary<string, DateTime> LockedLogicBlocks = new Dictionary<string, DateTime>();
        public bool AssignmentEnabled { get; set; } = true;
        public string Version { get; set; } = "1.0";
    }
}
