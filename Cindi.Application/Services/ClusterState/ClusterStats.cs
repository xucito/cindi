using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Services.ClusterState
{
    public class ClusterStats
    {
        public StepStats Steps { get; set; }
    }

    public class StepStats
    {
        public long Unassigned { get; set; }
        public long Assigned { get; set; }
        public long Suspended { get; set; }
        public long Successful { get; set; }
        public long Warning { get; set; }
        public long Error { get; set; }
    }
}
