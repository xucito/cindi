using Cindi.Domain.Entities.States;



using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Test.Global
{
    public static class Utility
    {
        public static Mock<IClusterRequestHandler> GetMockConsensusCoreNode()
        {
            return new Mock<IClusterRequestHandler>();
        }
    }
}
