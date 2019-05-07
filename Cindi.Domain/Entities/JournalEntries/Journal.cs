using Cindi.Domain.ValueObjects.Journal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cindi.Domain.Entities.JournalEntries
{
    public class Journal
    {
        public SortedDictionary<int, JournalEntry> JournalEntries;

        public Journal()
        {
            JournalEntries = new SortedDictionary<int, JournalEntry>();
        }

        public Journal(JournalEntry startingEntry)
        {
            JournalEntries = new SortedDictionary<int, JournalEntry>();
            AddJournalEntry(startingEntry);
        }

        public List<JournalEntry> Entries { get { return JournalEntries.Select(je => je.Value).ToList(); } }

        public T GetLatestValueOrDefault<T>(string fieldName, T defaultValue)
        {
            try
            {
                foreach (var entry in JournalEntries.Reverse())
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

        public List<UpdateRecord> GetAllUpdates(string fieldName)
        {
            var updates = new List<UpdateRecord>();


            foreach (var entry in JournalEntries.Reverse())
            {
                foreach (var update in entry.Value.Updates)
                {
                    if (update.FieldName == fieldName)
                    {
                        updates.Add(new UpdateRecord()
                        {
                            Update = update,
                            CreatedOn = entry.Value.CreatedOn
                        });
                    }
                }
            }
            return updates;
        }

        public int GetNextChainId()
        {
            if (JournalEntries.Count() == 0)
            {
                return 0;
            }
            return JournalEntries.Last().Key + 1;
        }

        public int GetCurrentChainId()
        {
            if (JournalEntries.Count() == 0)
            {
                return 0;
            }
            return JournalEntries.Last().Key;
        }

        public void AddJournalEntry(JournalEntry newEntry)
        {
            JournalEntries.Add(JournalEntries.Count(), newEntry);
        }

        public UpdateRecord GetLatestAction(string fieldName)
        {
            foreach (var entry in JournalEntries.Reverse())
            {
                foreach (var update in entry.Value.Updates)
                {
                    if (update.FieldName == fieldName)
                    {
                        return new UpdateRecord()
                        {
                            Update = update,
                            CreatedOn = entry.Value.CreatedOn
                        };
                    }
                }
            }
            return null;
        }
    }
}
