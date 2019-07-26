using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Workflows
{
    public class InvalidWorkflowProcessingException :BaseException
    {
        public InvalidWorkflowProcessingException()
        {
        }

        public InvalidWorkflowProcessingException(string message)
            : base(message)
        {
        }

        public InvalidWorkflowProcessingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
