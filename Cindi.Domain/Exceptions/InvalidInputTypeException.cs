using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions
{
    public class InvalidInputTypeException : Exception
    {
        public InvalidInputTypeException()
        {
        }

        public InvalidInputTypeException(string message)
            : base(message)
        {
        }

        public InvalidInputTypeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
