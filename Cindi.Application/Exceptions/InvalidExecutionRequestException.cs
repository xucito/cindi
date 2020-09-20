using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Exceptions
{
    public class InvalidExecutionRequestException : Exception
    {
        public InvalidExecutionRequestException()
        {
        }

        public InvalidExecutionRequestException(string message)
            : base(message)
        {
        }

        public InvalidExecutionRequestException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
