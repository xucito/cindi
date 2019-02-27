using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Cluster.Commands
{
    public class UpdateClusterStateCommand: IRequest<CommandResult>
    {
        public Dictionary<string, DateTime> StepTemplateAssignmentUpdates { get; set; }
    }
}
