using Cindi.Domain.Entities.Metrics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Application.Interfaces
{
    public interface IDatabaseMetricsCollector
    {
        Task<List<MetricTick>> GetMetricsAsync(Guid nodeId); 
    }
}
