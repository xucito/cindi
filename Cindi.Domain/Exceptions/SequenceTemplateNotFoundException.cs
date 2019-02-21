using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions
{
    public class SequenceTemplateNotFoundException: Exception
    {
        public SequenceTemplateNotFoundException()
        {
        }

        public SequenceTemplateNotFoundException(string message)
            : base(message)
        {
        }

        public SequenceTemplateNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
