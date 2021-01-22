using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.Metrics
{
    public class MetricTick : BaseEntity
    {
        public int MetricId { get; set; }
        public Guid ObjectId { get; set; }
        public double Value { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string SubCategory { get; set; }
    }
}
