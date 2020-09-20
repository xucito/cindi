using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.ValueObjects;
using ConsensusCore.Domain.BaseClasses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cindi.Domain.Entities
{
    public class TrackedEntity : ShardData
    {
        public TrackedEntity() { }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int Version { get; set; }
    }
}
