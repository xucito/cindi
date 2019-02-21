using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions
{
    public class ConflictingStepTestTemplateException : Exception
    {
        public ConflictingStepTestTemplateException()
        {
        }

        public ConflictingStepTestTemplateException(string message)
            : base(message)
        {
        }

        public ConflictingStepTestTemplateException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}