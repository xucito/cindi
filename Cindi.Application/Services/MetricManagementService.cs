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
        NodeStateService _nodeStateService;
        IEntitiesRepository _entitiesRepository;

        public MetricManagementService(ILogger<MetricManagementService> logger,
            IClusterRequestHandler node,
            NodeStateService nodeStateService,
            IEntitiesRepository entitiesRepository
            )
        {
            _logger = logger;
            _logger.LogInformation("Populating Metrics...");
            _metricLibrary = new MetricLibrary();
            _node = node;
            _nodeStateService = nodeStateService;
            _entitiesRepository = entitiesRepository;
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
                            var startTime = DateTime.Now;
                            await _node.Handle(new AddShardWriteOperation()
                            {
                                WaitForSafeWrite = true,
                                Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Create,
                                Data = tick,
                                Metric = false // Do not metric the metric write operations
                            });
                            _logger.LogDebug("Total write time took " + (DateTime.Now - startTime).TotalMilliseconds + " total ticks left in queue " + _ticks.Count());

                            if(_ticks.Count > 100)
                            {
                                _logger.LogWarning("Tick count is greater then 100...");
                            }
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
            var metrics = (await _entitiesRepository.GetAsync<Metric>(null, null, null, 100)).Select(m => m.MetricId);
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
