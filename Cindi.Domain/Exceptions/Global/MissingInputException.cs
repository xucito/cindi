using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Global
{
    public class MissingInputException :BaseException
    {
        public MissingInputException()
        {
        }

        public MissingInputException(string message)
            : base(message)
        {
        }

        public MissingInputException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
