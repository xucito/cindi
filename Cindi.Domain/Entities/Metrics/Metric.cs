using Cindi.Domain.Enums;

using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Entities.Metrics
{
    public class Metric: BaseEntity
    {
        /// <summary>
        /// Id of this metric
        /// </summary>
        public int MetricId { get; set; }
        /// <summary>
        /// What the type of metric was generated on
        /// </summary>
        public MetricType Type { get; set; }
        /// <summary>
        /// The type of the value used such as bytes per second
        /// </summary>
        public string ValueType { get; set; }
        public string MetricName { get; set; }
        /// <summary>
        /// Label used for the metric
        /// </summary>
        public string Label { get; set; }
    }
}
