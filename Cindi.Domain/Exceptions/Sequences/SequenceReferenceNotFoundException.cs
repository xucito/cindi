using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Sequences
{
    public class SequenceReferenceNotFoundException :BaseException
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
