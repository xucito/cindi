using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Exceptions
{
    public class InvalidCommandResultException : Exception
    {
        public InvalidCommandResultException()
        {
        }

        public InvalidCommandResultException(string message)
            : base(message)
        {
        }

        public InvalidCommandResultException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
