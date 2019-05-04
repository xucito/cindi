using Cindi.Domain.Exceptions.JournalEntries;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.JournalEntries
{
    public class JournalEntry
    {
        public List<Update> Updates { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        public JournalEntry()
        {}
    }
    public static class JournalEntityTypes
    {
        public const string Step = "step";
        public const string Sequence = "sequence";
    }
}
