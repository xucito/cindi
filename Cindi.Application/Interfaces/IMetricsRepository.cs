using System.Collections.Generic;
using System.Threading.Tasks;
using Cindi.Domain.Entities.Metrics;

namespace Cindi.Application.Interfaces
{
    public interface IMetricsRepository
    {
        Task<Metric> GetMetricAsync(int metricId);
        Task<IEnumerable<Metric>> GetMetricsAsync(int size = 10, int page = 0);
        Task<Metric> InsertMetricsAsync(Metric metrics);
    }
}