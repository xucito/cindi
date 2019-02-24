using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions
{
    public abstract class BaseException : Exception
    {
        public BaseException()
        {
        }

        public BaseException(string message)
            : base(message)
        {
        }

        public BaseException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public ExceptionResult ToExceptionResult(long elapsedMs)
        {
            return new ExceptionResult()
            {
                ExceptionName = this.GetType().Name,
                Generated = DateTime.UtcNow,
                Message = this.Message,
                ElapsedMs = elapsedMs
            };
        }
    }
}
