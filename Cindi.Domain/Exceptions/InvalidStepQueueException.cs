using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions
{
    public class InvalidStepQueueException : Exception
    {
        public InvalidStepQueueException()
        {
        }

        public InvalidStepQueueException(string message)
            : base(message)
        {
        }

        public InvalidStepQueueException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
