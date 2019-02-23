using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Steps
{
    public class InvalidStepStatusInputException :BaseException
    {
        public InvalidStepStatusInputException()
        {
        }

        public InvalidStepStatusInputException(string message)
            : base(message)
        {
        }

        public InvalidStepStatusInputException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
