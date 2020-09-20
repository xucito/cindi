using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Cluster.Commands.UpdateClusterState
{
    public class UpdateClusterStateCommand: IRequest<CommandResult>
    {
        public bool DefaultIfNull { get; set; } = false;
        public bool? AssignmentEnabled { get; set; } = null;
        public bool? AllowAutoRegistration { get; set; } = null;
        public string MetricRetentionPeriod { get; set; } = null;
    }
}
