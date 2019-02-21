using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions
{
    public class SequenceReferenceNotFoundException : Exception
    {
        public SequenceReferenceNotFoundException()
        {
        }

        public SequenceReferenceNotFoundException(string message)
            : base(message)
        {
        }

        public SequenceReferenceNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
