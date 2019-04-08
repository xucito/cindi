using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Sequences
{
    public class InvalidSequenceStatusException :BaseException
    {
        public InvalidSequenceStatusException()
        {
        }

        public InvalidSequenceStatusException(string message)
            : base(message)
        {
        }

        public InvalidSequenceStatusException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
