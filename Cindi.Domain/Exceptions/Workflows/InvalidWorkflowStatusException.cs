using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Workflows
{
    public class InvalidWorkflowStatusException :BaseException
    {
        public InvalidWorkflowStatusException()
        {
        }

        public InvalidWorkflowStatusException(string message)
            : base(message)
        {
        }

        public InvalidWorkflowStatusException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
