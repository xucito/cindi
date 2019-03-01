using Cindi.Domain.ValueObjects.Journal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cindi.Domain.Entities.JournalEntries
{
    public class Journal
    {
        private SortedList<int, JournalEntry> journalEntries;

        public Journal(List<JournalEntry> entries)
        {
            journalEntries = new SortedList<int, JournalEntry>();

            foreach (var entry in entries)
            {
                journalEntries.Add(entry.ChainId, entry);
            }
        }

        public List<JournalEntry> Entries { get { return journalEntries.Select(je => je.Value).ToList(); } }

        public T GetLatestValueOrDefault<T>(string fieldName, T defaultValue)
        {
            try
            {
                foreach (var entry in journalEntries.Reverse())
                {
                    foreach (var update in entry.Value.Updates)
                    {
                        if (update.FieldName == fieldName)
                        {
                            return (T)update.Value;
                        }
                    }
                }
                return defaultValue;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public int GetNextChainId()
        {
            if (journalEntries.Count() == 0)
            {
                return 0;
            }
            return journalEntries.Last().Key + 1;
        }

        public UpdateRecord GetLatestAction(string fieldName)
        {
            foreach (var entry in journalEntries.Reverse())
            {
                foreach (var update in entry.Value.Updates)
                {
                    if (update.FieldName == fieldName)
                    {
                        return new UpdateRecord()
                        {
                            Update = update,
                            RecordedOn = entry.Value.RecordedOn
                        };
                    }
                }
            }
            return null;
        }
    }
}
