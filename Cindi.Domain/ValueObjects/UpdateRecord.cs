using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.ValueObjects.Journal
{
    public class UpdateRecord
    {
        public DateTime RecordedOn { get; set; }
        public Update Update { get; set; }
    }
}
