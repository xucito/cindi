using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.GlobalValues
{
    public class InvalidGlobalValuesException : BaseException
    {
        public InvalidGlobalValuesException()
        {
        }

        public InvalidGlobalValuesException(string message)
            : base(message)
        {
        }

        public InvalidGlobalValuesException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
