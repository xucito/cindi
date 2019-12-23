using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Steps
{
    public class DuplicateStepUpdateException : BaseException
    {
        public DuplicateStepUpdateException()
        {
        }

        public DuplicateStepUpdateException(string message)
            : base(message)
        {
        }

        public DuplicateStepUpdateException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
