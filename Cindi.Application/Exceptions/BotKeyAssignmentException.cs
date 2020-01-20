using Cindi.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Exceptions
{
    public class BotKeyAssignmentException : BaseException
    {
        public BotKeyAssignmentException()
        {
        }

        public BotKeyAssignmentException(string message)
            : base(message)
        {
        }

        public BotKeyAssignmentException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
