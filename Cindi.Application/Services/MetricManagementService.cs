using Cindi.Application.Interfaces;
using Cindi.Application.SharedValues;
using Cindi.Domain.Entities.Metrics;
using Cindi.Domain.Entities.States;
using ConsensusCore.Domain.RPCs;
using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Node;
using ConsensusCore.Node.Communication.Controllers;
using ConsensusCore.Node.Services.Raft;
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
        IClusterRequestHandler _node;
        private readonly ConcurrentQueue<MetricTick> _ticks = new ConcurrentQueue<MetricTick>();
        private readonly Task writeThread;
        IMetricsRepository _metricsRepository;
        NodeStateService _nodeStateService;

        public MetricManagementService(ILogger<MetricManagementService> logger,
            IClusterRequestHandler node,
            IMetricsRepository metricsRepository,
            NodeStateService nodeStateService
            )
        {
            _logger = logger;
            _logger.LogInformation("Populating Metrics...");
            _metricLibrary = new MetricLibrary();
            _node = node;
            _nodeStateService = nodeStateService;
            _metricsRepository = metricsRepository;
            writeThread = new Task(async () =>
            {
                MetricTick tick;
                while (true)
                {
                   // Console.WriteLine("Number of tasks " + _ticks.Count());
                    if (_nodeStateService.InCluster)
                    {
                        if (_ticks.TryDequeue(out tick))
                        {
                            tick.Date = tick.Date.ToUniversalTime();
                            tick.Id = Guid.NewGuid();
                            await _node.Handle(new AddShardWriteOperation()
                            {
                                WaitForSafeWrite = true,
                                Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Create,
                                Data = tick,
                                Metric = false // Do not metric the metric write operations
                            });
                        }
                    }
                }
            });

            writeThread.Start();
        }


        public void EnqueueTick(MetricTick tick)
        {
            tick.Id = Guid.NewGuid();
            _ticks.Enqueue(tick);
        }

        public async void InitializeMetricStore()
        {
            var metrics = (await _metricsRepository.GetMetricsAsync(100, 0)).Select(m => m.MetricId);
            foreach (var metric in _metricLibrary.Metrics.Where(m => !metrics.Contains(m.Key)))
            {
                _logger.LogDebug("Adding metric " + metric.Key + " to database.");
                await _node.Handle(new AddShardWriteOperation()
                {
                    WaitForSafeWrite = true,
                    Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Create,
                    Data = metric.Value
                });
            };
        }

    }
}
