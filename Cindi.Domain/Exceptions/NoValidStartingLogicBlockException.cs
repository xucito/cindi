using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions
{
    public class NoValidStartingLogicBlockException : Exception
    {
        public NoValidStartingLogicBlockException()
        {
        }

        public NoValidStartingLogicBlockException(string message)
            : base(message)
        {
        }

        public NoValidStartingLogicBlockException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
