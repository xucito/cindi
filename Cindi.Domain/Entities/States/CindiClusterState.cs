using ConsensusCore.Domain.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cindi.Domain.Entities.States
{
    public class CindiClusterState : BaseState
    {
        public override void ApplyCommandToState(BaseCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
