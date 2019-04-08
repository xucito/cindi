using Cindi.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Exceptions
{
    public class InvalidClusterStateException : BaseException
    {
        public InvalidClusterStateException()
        {
        }

        public InvalidClusterStateException(string message)
            : base(message)
        {
        }

        public InvalidClusterStateException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
