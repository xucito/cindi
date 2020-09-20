using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Exceptions
{
    public class InvalidWorkflowTemplateException : Exception
    {
        public InvalidWorkflowTemplateException()
        {
        }

        public InvalidWorkflowTemplateException(string message)
            : base(message)
        {
        }

        public InvalidWorkflowTemplateException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
