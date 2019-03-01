using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Sequences
{
    public class MissingSequenceException : BaseException
    {
        public MissingSequenceException()
        {
        }

        public MissingSequenceException(string message)
            : base(message)
        {
        }

        public MissingSequenceException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
