using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.StepTemplates
{
    public class InputDefinitionNotFoundException : BaseException
    {
        public InputDefinitionNotFoundException()
        {
        }

        public InputDefinitionNotFoundException(string message)
            : base(message)
        {
        }

        public InputDefinitionNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
