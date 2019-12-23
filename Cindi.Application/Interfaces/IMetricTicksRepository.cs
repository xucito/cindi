using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cindi.Domain.Entities.Metrics;

namespace Cindi.Application.Interfaces
{
    public interface IMetricTicksRepository
    {
        Task<bool> DeleteMetricTicks(string ObjectId, DateTime toDate, int metricId);
        Task<Dictionary<int, SortedDictionary<DateTime, MetricTick>>> GetMetricTicksAsync(string objectId, DateTime fromDate, int[] metricIds);
        Task<MetricTick> InsertMetricTicksAsync(MetricTick metrics);
        Task<DateTime?> GetLastMetricTickDate(int metricDate);
    }
}