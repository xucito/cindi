using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Users
{
    public class ConflictingUsersException: BaseException
    {
        public ConflictingUsersException()
        {
        }

        public ConflictingUsersException(string message)
            : base(message)
        {
        }

        public ConflictingUsersException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
