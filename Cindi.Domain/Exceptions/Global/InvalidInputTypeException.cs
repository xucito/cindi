using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Global
{
    public class InvalidInputTypeException :BaseException
    {
        public InvalidInputTypeException()
        {
        }

        public InvalidInputTypeException(string message)
            : base(message)
        {
        }

        public InvalidInputTypeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
