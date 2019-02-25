using Cindi.Domain.Exceptions.JournalEntries;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.JournalEntries
{
    public class JournalEntry
    {
        private string _entity;
        public string Entity
        {
            get { return _entity; }
            set
            {
                if (value != JournalEntityTypes.Step
                     && value != JournalEntityTypes.Sequence)
                {
                    throw new InvalidJournalEntryEntityTypeException("Entity type " + value + " is not a valid entity.");
                }
                _entity = value;
            }
        }
        public Guid SubjectId { get; set; }
        public DateTime RecordedOn { get; set; }
        public string Id { get { return _entity + ":" + SubjectId + ":" + ChainId; } }
        public int ChainId { get; set; }
        public List<Update> Updates { get; set; }

        public JournalEntry()
        {}
    }
    public static class JournalEntityTypes
    {
        public const string Step = "step";
        public const string Sequence = "sequence";
    }
}
