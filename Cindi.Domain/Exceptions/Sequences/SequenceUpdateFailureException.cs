using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Sequences
{
    public class SequenceUpdateFailureException : BaseException
    {
        public SequenceUpdateFailureException()
        {
        }

        public SequenceUpdateFailureException(string message)
            : base(message)
        {
        }

        public SequenceUpdateFailureException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
