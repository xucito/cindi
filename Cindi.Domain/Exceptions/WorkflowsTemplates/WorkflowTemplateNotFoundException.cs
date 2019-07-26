using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.WorkflowTemplates
{
    public class WorkflowTemplateNotFoundException:BaseException
    {
        public WorkflowTemplateNotFoundException()
        {
        }

        public WorkflowTemplateNotFoundException(string message)
            : base(message)
        {
        }

        public WorkflowTemplateNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
