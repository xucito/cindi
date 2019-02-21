using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions
{
    public class StepTemplateDuplicateFoundException : Exception
    {
        public StepTemplateDuplicateFoundException()
        {
        }

        public StepTemplateDuplicateFoundException(string message)
            : base(message)
        {
        }

        public StepTemplateDuplicateFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
