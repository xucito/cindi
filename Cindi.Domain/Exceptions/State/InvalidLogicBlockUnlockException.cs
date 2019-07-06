using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.State
{
    public class InvalidLogicBlockUnlockException : BaseException
    {
        public InvalidLogicBlockUnlockException()
        {
        }

        public InvalidLogicBlockUnlockException(string message)
            : base(message)
        {
        }

        public InvalidLogicBlockUnlockException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
