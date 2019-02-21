using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions
{
    public class AtomicOperationException: Exception
    {
        public AtomicOperationException()
        {
        }

        public AtomicOperationException(string message)
            : base(message)
        {
        }

        public AtomicOperationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
