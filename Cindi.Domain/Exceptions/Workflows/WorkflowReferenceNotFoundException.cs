using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Workflows
{
    public class WorkflowReferenceNotFoundException :BaseException
    {
        public WorkflowReferenceNotFoundException()
        {
        }

        public WorkflowReferenceNotFoundException(string message)
            : base(message)
        {
        }

        public WorkflowReferenceNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
