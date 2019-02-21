using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions
{
    public class MissingInputException : Exception
    {
        public MissingInputException()
        {
        }

        public MissingInputException(string message)
            : base(message)
        {
        }

        public MissingInputException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
