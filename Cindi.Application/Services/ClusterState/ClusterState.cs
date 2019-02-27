using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Services.ClusterState
{
    public class ClusterState
    {
        public const string DefaultId = "State";
        public string Id { get; } = DefaultId;
        public Dictionary<string, DateTime> StepAssignmentCheckpoints = new Dictionary<string, DateTime>();
    }
}
