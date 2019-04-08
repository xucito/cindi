using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.JournalEntries
{
    public class InvalidJournalEntryEntityTypeException : BaseException
    {
        public InvalidJournalEntryEntityTypeException()
        {
        }

        public InvalidJournalEntryEntityTypeException(string message)
            : base(message)
        {
        }

        public InvalidJournalEntryEntityTypeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
