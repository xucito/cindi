using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Global
{
    public class InvalidValueException : BaseException
    {
        public InvalidValueException()
        {
        }

        public InvalidValueException(string message)
            : base(message)
        {
        }

        public InvalidValueException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
