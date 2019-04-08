using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Sequences
{
    public class InvalidSequenceProcessingException :BaseException
    {
        public InvalidSequenceProcessingException()
        {
        }

        public InvalidSequenceProcessingException(string message)
            : base(message)
        {
        }

        public InvalidSequenceProcessingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
