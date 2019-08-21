using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Workflows
{
    public class InvalidInputsException : BaseException
    {
        public InvalidInputsException()
        {
        }

        public InvalidInputsException(string message)
            : base(message)
        {
        }

        public InvalidInputsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
