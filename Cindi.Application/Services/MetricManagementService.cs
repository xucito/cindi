using Cindi.Application.Interfaces;
using Cindi.Application.SharedValues;
using Cindi.Domain.Entities.Metrics;
using Cindi.Domain.Entities.States;
using Nest;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Services
{
    public class MetricManagementService
    {
        readonly MetricLibrary _metricLibrary;
        ILogger<MetricManagementService> _logger;
        ElasticClient _context;
        private readonly ConcurrentQueue<MetricTick> _ticks = new ConcurrentQueue<MetricTick>();
        private readonly Task writeThread;
        IConfiguration _configuration;
        bool EnableMetrics = false;

        public MetricManagementService(
            ILogger<MetricManagementService> logger,
            ElasticClient context,
            IConfiguration configuration
            )
        {
            _logger = logger;
            _logger.LogInformation("Populating Metrics...");
            _metricLibrary = new MetricLibrary();
            _context = context;
            _configuration = configuration;
            EnableMetrics = _configuration.GetValue<bool>("EnableMonitoring");
            if (EnableMetrics)
            {
                writeThread = new Task(async () =>
                {
                    MetricTick tick;
                    while (true)
                    {
                        // Console.WriteLine("Number of tasks " + _ticks.Count());
                        if (_ticks.TryDequeue(out tick))
                        {
                            tick.Date = tick.Date.ToUniversalTime();
                            tick.Id = Guid.NewGuid();
                            var startTime = DateTimeOffset.UtcNow;
                            await _context.IndexDocumentAsync(tick);
                            
                            _logger.LogDebug("Total write time took " + (DateTimeOffset.UtcNow - startTime).TotalMilliseconds + " total ticks left in queue " + _ticks.Count());

                            if (_ticks.Count > 100)
                            {
                                _logger.LogWarning("Tick count is greater then 100...");
                            }
                        }
                        await Task.Delay(10);
                    }
                });

                writeThread.Start();
            }
        }


        public void EnqueueTick(MetricTick tick)
        {
            if (EnableMetrics)
            {
                tick.Id = Guid.NewGuid();
                _ticks.Enqueue(tick);
            }
        }

        public async void InitializeMetricStore()
        {
            var metrics = ((await _context.SearchAsync<Metric>()).Hits).Select(m => m.Source.MetricId);
            foreach (var metric in _metricLibrary.Metrics.Where(m => !metrics.Contains(m.Key)))
            {
                _logger.LogDebug("Adding metric " + metric.Key + " to database.");
                await _context.IndexDocumentAsync(metric.Value);
                
            };
        }

    }
}
