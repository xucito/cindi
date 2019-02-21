using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions
{
    public class MissingStartingStepException : Exception
    {
        public MissingStartingStepException()
        {
        }

        public MissingStartingStepException(string message)
            : base(message)
        {
        }

        public MissingStartingStepException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
