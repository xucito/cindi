using ConsensusCore.Domain.BaseClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.ClusterCommands
{
    public class ResetClusterSettings : BaseCommand
    {
        public override string CommandName => "ResetClusterSettings";
    }
}
