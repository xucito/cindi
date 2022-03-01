using Cindi.Application.Results;
using Cindi.Domain.Entities.Metrics;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Metrics.Queries.GetMetrics
{
    public class GetMetricsQuery : IRequest<QueryResult<object>>
    {
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
        /// <summary>
        /// Use metricname.subcategory to use subcategories
        /// </summary>
        public string MetricName { get; set; }
        public string SubCategory { get; set; } = null;
        public Guid ObjectId { get; set; }
        public string[] Aggs { get; set; }
        /// <summary>
        /// Y = year
        /// m = month
        /// d = Date
        /// H = hour
        /// M = minute
        /// S = seconds
        /// </summary>
        public char Interval { get; set; } = 'S';
        public bool IncludeSubcategories { get; set; }
    }
}
