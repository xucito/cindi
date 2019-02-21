using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions
{
    public class InvalidSequenceStatusException : Exception
    {
        public InvalidSequenceStatusException()
        {
        }

        public InvalidSequenceStatusException(string message)
            : base(message)
        {
        }

        public InvalidSequenceStatusException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
