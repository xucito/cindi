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
            AddMetric(SchedulerLatencyMs);
            AddMetric(DatabaseOperationsPerSecond);
            AddMetric(DatabaseTotalSizeBytes);
            AddMetric(CPUUsagePercent);
            AddMetric(StepCacheRefreshTimeMs);
        }

        public void AddMetric(Metric metric)
        {
            Metrics.Add(Metrics.Count, metric);
        }

        public static Metric GetMetric(MetricIds id, string label, string metricName, MetricType type, string valueType)
        {
            return new Metric()
            {
                MetricId = (int)id,
                Label = label,
                MetricName = metricName,
                Type = type,
                ValueType = valueType
            };
        }

        public static Metric QueuedStepsPerSecond { get { return GetMetric(MetricIds.QueuedStepsPerSecond, "Queued Steps per Second", "queuedstepspersecond", MetricType.Cluster, "Number per second"); } }
        public static Metric ClusterOperationElapsedMs { get { return GetMetric(MetricIds.ClusterOperationElapsedMs, "Cluster Operation Elapsed Ms", "clusteroperationelapsedms", MetricType.Cluster, "Total Elapsed ms"); } }
        public static Metric DatabaseOperationLatencyMs { get { return GetMetric(MetricIds.DatabaseOperationLatency, "Database Operation Latency Ms", "databaseoperationlatencyms", MetricType.Node, "Total Elapsed ms"); } }
        public static Metric DatabaseOperationCount { get { return GetMetric(MetricIds.DatabaseOperationCount, "Database Operation Count", "databaseoperationcount", MetricType.Node, "Total Operation Count"); } }
        public static Metric SchedulerLatencyMs { get { return GetMetric(MetricIds.SchedulerLatencyMs, "Delay in the scheduler intervals", "schedulerlatency", MetricType.Cluster, "Total Elapsed ms"); } }
        public static Metric DatabaseOperationsPerSecond { get { return GetMetric(MetricIds.DatabaseOperationsPerSecond, "Database Operations per Second", "databaseoperationspersecond", MetricType.Node, "Number per second"); } }
        public static Metric DatabaseTotalSizeBytes { get { return GetMetric(MetricIds.DatabaseTotalSizeBytes, "Database Total Size in Bytes", "databasetotalsizebytes", MetricType.Node, "Total in Bytes"); } }
        public static Metric CPUUsagePercent { get { return GetMetric(MetricIds.CPUUsagePercent, "CPU Usage in Percent", "cpuusagepercent", MetricType.Node, "Total in %"); } }
        public static Metric StepCacheRefreshTimeMs { get { return GetMetric(MetricIds.StepCacheRefreshTimeMs, "Step Cache Refresh Time Ms", "stepcacherefreshms", MetricType.Node, "Total in ms"); } }

    }
}
