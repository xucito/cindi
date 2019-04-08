using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.StepTemplates
{
    public class MissingStartingStepException :BaseException
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
