using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Steps
{
   public  class StepUpdateFailureException : BaseException
    {
        public StepUpdateFailureException()
        {
        }

        public StepUpdateFailureException(string message)
            : base(message)
        {
        }

        public StepUpdateFailureException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
