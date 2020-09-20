using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Exceptions
{
    public class InvalidExecutionScheduleException : Exception
    {
        public InvalidExecutionScheduleException()
        {
        }

        public InvalidExecutionScheduleException(string message)
            : base(message)
        {
        }

        public InvalidExecutionScheduleException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
