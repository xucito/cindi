using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Utility
{
    public class InvalidPrivateKeyException : BaseException
    {
        public InvalidPrivateKeyException()
        {
        }

        public InvalidPrivateKeyException(string message)
            : base(message)
        {
        }

        public InvalidPrivateKeyException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}