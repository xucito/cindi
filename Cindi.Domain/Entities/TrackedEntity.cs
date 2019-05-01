using Cindi.Domain.Entities.JournalEntries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities
{
    public class TrackedEntity
    {
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Journal Journal { get; set; }
    }
}
