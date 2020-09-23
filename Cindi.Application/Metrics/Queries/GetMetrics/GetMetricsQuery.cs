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
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        //Name of the metric and the aggregations used, max, avg, min, max
        public Dictionary<string, string[]> Metrics { get; set; }
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
