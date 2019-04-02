using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Cluster.Commands.UpdateClusterState
{
    public class UpdateClusterStateCommand: IRequest<CommandResult>
    {
        public bool? AssignmentEnabled { get; set; } = true;
        public bool? AutoRegistrationEnabled { get; set; }
    }
}
