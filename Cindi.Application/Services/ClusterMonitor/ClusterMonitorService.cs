using Cindi.Application.Interfaces;
using Cindi.Application.Services.ClusterState;
using Cindi.Application.Steps.Commands.UnassignStep;
using Cindi.Application.Steps.Queries.GetSteps;
using Cindi.Domain.Entities.Metrics;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Node;
using ConsensusCore.Node.Communication.Controllers;
using ConsensusCore.Node.Services.Raft;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Services.ClusterMonitor
{
    public class ClusterMonitorService
    {
        private IMediator _mediator;
        Thread checkSuspendedStepsThread;
        private ILogger<ClusterMonitorService> _logger;
        private IClusterRequestHandler node;
        private IStepsRepository _stepsRepository;
        private IMetricTicksRepository _metricTicksRepository;
        private MetricManagementService _metricManagementService;
        int leaderMonitoringInterval = Timeout.Infinite;
        int nodeMonitoringInterval = Timeout.Infinite;
        int lastSecond = 0;
        private IDatabaseMetricsCollector _databaseMetricsCollector;
        private NodeStateService _nodeStateService;

        private int _fetchingDbMetrics = 0; //0 is false, 1 is true

        private Timer monitoringTimer;
        private const int secondsOfMetrics = 5;

        public ClusterMonitorService(
            IServiceScopeFactory serviceProvider,
            IClusterRequestHandler _node,
            IStepsRepository stepsRepository,
            MetricManagementService metricManagementService,
            IMetricTicksRepository metricTicksRepository,
            IDatabaseMetricsCollector databaseMetricsCollector,
            NodeStateService nodeStateService)
        {
            _metricManagementService = metricManagementService;
            var sp = serviceProvider.CreateScope().ServiceProvider;
            _mediator = sp.GetService<IMediator>();
            _logger = sp.GetService<ILogger<ClusterMonitorService>>();

            _logger.LogInformation("Starting clean up service...");
            node = _node;
            _stepsRepository = stepsRepository;
            _metricTicksRepository = metricTicksRepository;
            _databaseMetricsCollector = databaseMetricsCollector;
            monitoringTimer = new System.Threading.Timer(CollectMetricsEventHandler);
            node.MetricGenerated += metricGenerated;
            _nodeStateService = nodeStateService;
            Start();
        }

        /*public ClusterMonitorService(IMediator mediatr, ILogger<ClusterMonitorService> logger)
        {
            _mediator = mediatr;
            _logger = logger;
            Start();
        }*/

        public void Start()
        {
            monitoringTimer.Change(0, 10);
            checkSuspendedStepsThread = new Thread(async () =>
            {
                bool printedMessage = false;
                while (true)
                {
                    //Do not run if it is uninitialized
                    if (ClusterStateService.Initialized && _nodeStateService.Role == ConsensusCore.Domain.Enums.NodeState.Leader)
                    {
                        if (!printedMessage)
                        {
                            _logger.LogInformation("Detected I am cluster leader, starting to clean up cluster");
                            printedMessage = true;
                        }
                        try
                        {
                            var page = 0;
                            long stepPosition = 0;
                            long totalSteps = 0;
                            int cleanedCount = 0;
                            do
                            {
                                var steps = await _mediator.Send(new GetStepsQuery
                                {
                                    Page = 0,
                                    Size = 1000,
                                    Status = StepStatuses.Suspended
                                });

                                totalSteps = steps.Count.Value;
                                stepPosition += steps.Count.Value;
                                page++;

                                foreach (var step in steps.Result)
                                {
                                    if (step.SuspendedUntil != null && step.SuspendedUntil < DateTime.UtcNow)
                                    {
                                        await _mediator.Send(new UnassignStepCommand
                                        {
                                            StepId = step.Id
                                        });
                                        cleanedCount++;
                                    }
                                }
                            }
                            while (stepPosition < totalSteps);
                            if (cleanedCount > 0)
                            {
                                _logger.LogInformation("Cleaned " + cleanedCount + " steps");
                            }
                            Thread.Sleep(1000);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError("Failed to check suspended threads with exception " + e.Message + Environment.NewLine + e.StackTrace);
                        }
                    }
                    else
                    {
                        printedMessage = false;
                        Thread.Sleep(3000);
                    }
                }
            });
            checkSuspendedStepsThread.Start();
        }

        public async void CollectMetricsEventHandler(object args)
        {
            var currentDateTime = DateTime.Now;
            if (lastSecond != currentDateTime.Second && currentDateTime.Second % secondsOfMetrics == 0)
            {
                lastSecond = currentDateTime.Second;
                var truncatedTime = currentDateTime;
                truncatedTime = truncatedTime.AddTicks(-(truncatedTime.Ticks % TimeSpan.TicksPerSecond));
                // Console.WriteLine(truncatedTime.ToString("o"));
                //  Console.WriteLine(currentDateTime.ToString("o"));

                DateTime toDate = truncatedTime;
                DateTime fromDate = truncatedTime.AddSeconds(-secondsOfMetrics);

                if (_nodeStateService.Role == ConsensusCore.Domain.Enums.NodeState.Leader)
                {
                    var stepsCount = (await _stepsRepository.GetStepsAsync(fromDate, toDate)).ToList().Count();
                    _metricManagementService.EnqueueTick(new MetricTick()
                    {
                        MetricId = 0,
                        Date = truncatedTime,
                        Value = stepsCount
                    });
                }

                if (Interlocked.CompareExchange(ref _fetchingDbMetrics, 1, 0) == 0)
                {
                    Console.WriteLine("Enter");
                    foreach(var metric in await _databaseMetricsCollector.GetMetricsAsync(_nodeStateService.Id))
                    {
                        _metricManagementService.EnqueueTick(metric);
                    }
                    Interlocked.Decrement(ref _fetchingDbMetrics);
                    Console.WriteLine("Exit");
                }
               // Console.WriteLine("Writing metrics from " + fromDate.ToString("o") + " to " + toDate.ToString("o") + " value:" + stepsCount);
            }
            /*
            DateTime toDate = DateTime.Now;
            _metricManagementService.EnqueueTick(new MetricTick()
            {
                MetricId = 0,
                Date = toDate,
                Value = (await _stepsRepository.GetStepsAsync(toDate.AddSeconds(1), toDate)).ToList().Count()
            });
            Console.WriteLine("Finished thread");*/
        }

        void metricGenerated(object sender, ConsensusCore.Domain.Models.Metric e)
        {
            _metricManagementService.EnqueueTick(new MetricTick()
            {
                MetricId = 1,
                Date = e.Date,
                Value = e.Value,
                SubCategory = e.Type.SubCategory
            });
        }
    }
}
