using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Workflows
{
    public class WorkflowUpdateFailureException : BaseException
    {
        public WorkflowUpdateFailureException()
        {
        }

        public WorkflowUpdateFailureException(string message)
            : base(message)
        {
        }

        public WorkflowUpdateFailureException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
