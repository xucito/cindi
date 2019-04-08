using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Utility
{
    public class InvalidEncryptionProtocolException : BaseException
    {
        public InvalidEncryptionProtocolException()
        {
        }

        public InvalidEncryptionProtocolException(string message)
            : base(message)
        {
        }

        public InvalidEncryptionProtocolException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
