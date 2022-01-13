using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.GlobalValues
{
    public class GlobalValue: TrackedEntity
    {
        public string Name { get; set; }
        public int Version { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public object Value { get; set; }
        public string Status { get; set; }
    }
}
