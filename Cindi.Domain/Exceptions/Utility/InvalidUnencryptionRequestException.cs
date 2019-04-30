using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Utility
{
    public class InvalidUnencryptionRequestException : BaseException
    {
        public InvalidUnencryptionRequestException()
        {
        }

        public InvalidUnencryptionRequestException(string message)
            : base(message)
        {
        }

        public InvalidUnencryptionRequestException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
