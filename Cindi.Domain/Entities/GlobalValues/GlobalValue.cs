using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.GlobalValues
{
    public class GlobalValue: TrackedEntity
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Description { get { return Journal.GetLatestValueOrDefault<string>("description", ""); } }

        public object Value
        {
            get { return Journal.GetLatestValueOrDefault<object>("value", null); }
        }

        public string Status { get; set; }

        public Guid Id { get; set; }
    }
}
