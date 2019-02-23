using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Steps
{
    public class InvalidStepQueueException :BaseException
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
