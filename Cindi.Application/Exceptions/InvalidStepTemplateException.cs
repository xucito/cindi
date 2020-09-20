using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Exceptions
{
    public class InvalidStepTemplateException : Exception
    {
        public InvalidStepTemplateException()
        {
        }

        public InvalidStepTemplateException(string message)
            : base(message)
        {
        }

        public InvalidStepTemplateException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
