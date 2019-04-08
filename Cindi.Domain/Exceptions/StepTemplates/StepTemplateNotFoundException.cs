using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.StepTemplates
{
    public class StepTemplateNotFoundException:BaseException
    {
        public StepTemplateNotFoundException()
        {
        }

        public StepTemplateNotFoundException(string message)
            : base(message)
        {
        }

        public StepTemplateNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
