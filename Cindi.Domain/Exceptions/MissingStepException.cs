using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions
{
    public class MissingStepException : Exception
    {
        public MissingStepException()
        {
        }

        public MissingStepException(string message)
            : base(message)
        {
        }

        public MissingStepException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
