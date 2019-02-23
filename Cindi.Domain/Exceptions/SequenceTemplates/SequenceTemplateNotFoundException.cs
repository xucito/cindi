using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.SequenceTemplates
{
    public class SequenceTemplateNotFoundException:BaseException
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
