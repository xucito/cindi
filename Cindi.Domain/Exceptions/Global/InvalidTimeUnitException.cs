using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Global
{
    public class InvalidTimeUnitException : BaseException
    {
        public InvalidTimeUnitException()
        {
        }

        public InvalidTimeUnitException(string message)
            : base(message)
        {
        }

        public InvalidTimeUnitException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
