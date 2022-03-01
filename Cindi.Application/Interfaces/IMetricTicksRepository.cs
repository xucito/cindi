using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cindi.Domain.Entities.Metrics;

namespace Cindi.Application.Interfaces
{
    public interface IMetricTicksRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="metricIdsAndSubcategories">[metric Id]-[subcategory]</param>
        /// <returns></returns>
        Task<object> GetMetricTicksAsync(DateTimeOffset fromDate, DateTimeOffset toDate, int metricId, string[] aggs, char interval = 'S', string subcategory = null, Guid? objectId = null, bool includeSubcategories = false);
        Task<MetricTick> InsertMetricTicksAsync(MetricTick metrics);
        Task<DateTimeOffset?> GetLastMetricTickDate(int metricDate);
    }
}