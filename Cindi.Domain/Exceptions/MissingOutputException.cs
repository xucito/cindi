using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions
{
    public class MissingOutputException : Exception
    {
        public MissingOutputException()
        {
        }

        public MissingOutputException(string message)
            : base(message)
        {
        }

        public MissingOutputException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
