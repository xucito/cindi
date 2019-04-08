using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.BotKeys
{
    public class ConflictingBotKeyException: BaseException
    {
        public ConflictingBotKeyException()
        {
        }

        public ConflictingBotKeyException(string message)
            : base(message)
        {
        }

        public ConflictingBotKeyException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
