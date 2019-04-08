using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Steps
{
    public class MissingStepException :BaseException
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
