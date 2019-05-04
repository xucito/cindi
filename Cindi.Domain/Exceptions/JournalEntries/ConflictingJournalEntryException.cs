using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.JournalEntries
{
    public class ConflictingJournalEntryException: BaseException
    {
        public ConflictingJournalEntryException()
        {
        }

        public ConflictingJournalEntryException(string message)
            : base(message)
        {
        }

        public ConflictingJournalEntryException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
