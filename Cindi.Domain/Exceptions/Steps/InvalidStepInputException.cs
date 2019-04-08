using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Steps
{
    public class InvalidStepInputException: BaseException
    {
        public InvalidStepInputException()
        {
        }

        public InvalidStepInputException(string message)
            : base(message)
        {
        }

        public InvalidStepInputException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
