using Cindi.Domain.Entities.Metrics;
using Cindi.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.SharedValues
{
    public class MetricLibrary
    {
        public SortedDictionary<int, Metric> Metrics = new SortedDictionary<int, Metric>();

        public MetricLibrary()
        {
            AddMetric(QueuedStepsPerSecond);
            AddMetric(ClusterOperationElapsedMs);
            AddMetric(DatabaseOperationLatencyMs);
            AddMetric(DatabaseOperationCount);
        }

        public void AddMetric(Metric metric)
        {
            Metrics.Add(Metrics.Count, metric);
        }

        public static Metric GetMetric(int id, string label, string metricName, MetricType type, string valueType)
        {
            return new Metric()
            {
                MetricId = id,
                Label = label,
                MetricName = metricName,
                Type = type,
                ValueType = valueType
            };
        }

        public static Metric QueuedStepsPerSecond { get { return GetMetric(0, "Queued Steps per Second", "queuedstepspersecond", MetricType.Cluster, "Number per seconds");  } }
        public static Metric ClusterOperationElapsedMs { get { return GetMetric(1, "Cluster Operation Elapsed Ms", "clusteroperationelapsedms", MetricType.Cluster, "Total Elapsed ms"); } }
        public static Metric DatabaseOperationLatencyMs { get { return GetMetric(2, "Database Operation Latency Ms", "databaseoperationlatencyms", MetricType.Node, "Total Elapsed ms"); } }
        public static Metric DatabaseOperationCount { get { return GetMetric(2, "Database Operation Count", "databaseoperationcount", MetricType.Node, "Total Operation Count"); } }
    }
}
