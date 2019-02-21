using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions
{
    public class InvalidInputValueException : Exception
    {
        public InvalidInputValueException()
        {
        }

        public InvalidInputValueException(string message)
            : base(message)
        {
        }

        public InvalidInputValueException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
