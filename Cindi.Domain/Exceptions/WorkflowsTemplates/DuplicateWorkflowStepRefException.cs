using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.WorkflowTemplates
{
    public class DuplicateWorkflowStepRefException :BaseException
    {
        public DuplicateWorkflowStepRefException()
        {
        }

        public DuplicateWorkflowStepRefException(string message)
            : base(message)
        {
        }

        public DuplicateWorkflowStepRefException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
