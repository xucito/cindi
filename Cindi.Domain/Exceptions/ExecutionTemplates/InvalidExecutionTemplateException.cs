using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Global
{
    public class InvalidExecutionTemplateException : BaseException
    {
        public InvalidExecutionTemplateException()
        {
        }

        public InvalidExecutionTemplateException(string message)
            : base(message)
        {
        }

        public InvalidExecutionTemplateException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
