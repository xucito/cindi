using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Steps
{
   public class InvalidStepStatusException : BaseException
    {
        public InvalidStepStatusException()
        {
        }

        public InvalidStepStatusException(string message)
            : base(message)
        {
        }

        public InvalidStepStatusException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
