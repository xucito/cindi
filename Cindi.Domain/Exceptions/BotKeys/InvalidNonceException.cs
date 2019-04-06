using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.BotKeys
{
    public class InvalidNonceException : BaseException
    {
        public InvalidNonceException()
        {
        }

        public InvalidNonceException(string message)
            : base(message)
        {
        }

        public InvalidNonceException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
