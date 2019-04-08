using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.StepTemplates
{
    public class ConflictingStepTemplateException :BaseException
    {
        public ConflictingStepTemplateException()
        {
        }

        public ConflictingStepTemplateException(string message)
            : base(message)
        {
        }

        public ConflictingStepTemplateException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
