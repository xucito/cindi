using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.GlobalValues
{
    public class DuplicateGlobalValueException : BaseException
    {
        public DuplicateGlobalValueException()
        {
        }

        public DuplicateGlobalValueException(string message)
            : base(message)
        {
        }

        public DuplicateGlobalValueException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
