using Cindi.Domain.Entities.States;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Node;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Test.Global
{
    public static class Utility
    {
        public static Mock<IConsensusCoreNode<CindiClusterState, IBaseRepository<CindiClusterState>>> GetMockConsensusCoreNode()
        {
            return new Mock<IConsensusCoreNode<CindiClusterState, IBaseRepository<CindiClusterState>>>();
        }
    }
}
