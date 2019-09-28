using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.State
{
    public class FailedClusterOperationException : BaseException
    {
        public FailedClusterOperationException()
        {
        }

        public FailedClusterOperationException(string message)
            : base(message)
        {
        }

        public FailedClusterOperationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
