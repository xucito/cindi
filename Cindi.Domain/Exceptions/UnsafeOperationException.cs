using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions
{
    public class UnsafeOperationException : Exception
    {
        public UnsafeOperationException()
        {
        }

        public UnsafeOperationException(string message)
            : base(message)
        {
        }

        public UnsafeOperationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
