using Cindi.Application.Interfaces;
using Cindi.Application.SharedValues;
using Cindi.Domain.Entities.Metrics;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Utilities;
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
        private readonly ConcurrentQueue<MetricTick> _ticks = new ConcurrentQueue<MetricTick>();
        private readonly Task writeThread;
        IEntitiesRepository _entitiesRepository;
        IConfiguration _configuration;
        bool EnableMetrics = false;

        public MetricManagementService(
            ILogger<MetricManagementService> logger,
            IEntitiesRepository entitiesRepository,
            IConfiguration configuration
            )
        {
            _logger = logger;
            _logger.LogInformation("Populating Metrics...");
            _metricLibrary = new MetricLibrary();
            _entitiesRepository = entitiesRepository;
            _configuration = configuration;
            EnableMetrics = _configuration.GetValue<bool>("EnableMonitoring");
            if (EnableMetrics)
            {
                writeThread = new Task(async () =>
                {
                    while (true)
                    {
                        List<MetricTick> items = _ticks.DequeueChunk(100).Select(i =>
                       {
                           i.Id = Guid.NewGuid();
                           return i;
                       }).ToList();
                        if (items.Count() > 0)
                        {
                            await _entitiesRepository.InsertMany(items);
                        }
                        var startTime = DateTime.Now;
                        _logger.LogDebug("Total write time took " + (DateTime.Now - startTime).TotalMilliseconds + " total ticks left in queue " + _ticks.Count());
                        await Task.Delay(1000);
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
            var metrics = (await _entitiesRepository.GetAsync<Metric>(null, null, null, 100)).Select(m => m.MetricId);
            await _entitiesRepository.InsertMany(_metricLibrary.Metrics.Where(m => !metrics.Contains(m.Key)).Select(_ => _.Value));
        }

    }
}
