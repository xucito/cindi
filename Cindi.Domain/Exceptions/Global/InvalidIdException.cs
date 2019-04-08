using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Global
{
    public class InvalidIdException: BaseException
    {
        public InvalidIdException()
        {
        }

        public InvalidIdException(string message)
            : base(message)
        {
        }

        public InvalidIdException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}