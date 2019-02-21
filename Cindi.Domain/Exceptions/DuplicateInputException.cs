using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions
{
    public class DuplicateInputException : Exception
    {
        public DuplicateInputException()
        {
        }

        public DuplicateInputException(string message)
            : base(message)
        {
        }

        public DuplicateInputException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
