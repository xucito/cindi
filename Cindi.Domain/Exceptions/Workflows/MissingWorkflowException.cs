using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Workflows
{
    public class MissingWorkflowException : BaseException
    {
        public MissingWorkflowException()
        {
        }

        public MissingWorkflowException(string message)
            : base(message)
        {
        }

        public MissingWorkflowException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
