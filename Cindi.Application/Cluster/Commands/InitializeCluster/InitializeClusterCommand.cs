using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Cluster.Commands.InitializeCluster
{
    public class InitializeClusterCommand: IRequest<CommandResult<NewClusterResult>>
    {
        public string Name { get; set; }
        public string DefaultPassword { get; set; }
    }
}
