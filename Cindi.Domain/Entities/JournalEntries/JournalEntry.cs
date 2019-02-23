using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.JournalEntries
{
    public class JournalEntry
    {
        public Guid SubjectId { get; set; }
        public DateTime RecordedOn { get; set; }
        public int Id { get; set; }
        public List<Update> Updates { get; set; }
    }
}
