using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Steps
{
    public class InvalidStepPermissionException : BaseException
    {
        public InvalidStepPermissionException()
        {
        }

        public InvalidStepPermissionException(string message)
            : base(message)
        {
        }

        public InvalidStepPermissionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
