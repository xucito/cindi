﻿using Cindi.Application.Entities.Queries;
using Cindi.Application.ExecutionSchedules.Commands.RecalculateExecutionSchedule;
using Cindi.Application.ExecutionTemplates.Commands.ExecuteExecutionTemplate;
using Cindi.Application.Interfaces;
using Cindi.Application.Services.ClusterState;
using Cindi.Application.Steps.Commands.UnassignStep;
using Cindi.Domain.Entities.ExecutionSchedule;
using Cindi.Domain.Entities.Metrics;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Enums;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.SystemCommands;
using ConsensusCore.Node;
using ConsensusCore.Node.Communication.Controllers;
using ConsensusCore.Node.Services.Raft;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
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
        private IEntitiesRepository _entitiesRepository;
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
            IServiceProvider sp,
            IClusterRequestHandler _node,
            IEntitiesRepository entitiesRepository,
            MetricManagementService metricManagementService,
            IMetricTicksRepository metricTicksRepository,
            IDatabaseMetricsCollector databaseMetricsCollector,
            NodeStateService nodeStateService)
        {
            _metricManagementService = metricManagementService;
            // var sp = serviceProvider.CreateScope().ServiceProvider;
            _mediator = sp.GetService<IMediator>();
            _logger = sp.GetService<ILogger<ClusterMonitorService>>();

            _logger.LogInformation("Starting clean up service...");
            node = _node;
            _entitiesRepository = entitiesRepository;
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
            checkSuspendedStepsThread = new Thread(async () => await CheckSuspendedSteps());
            checkSuspendedStepsThread.Start();
            checkSuspendedStepsThread = new Thread(async () => await CheckScheduledExecutions());
            checkSuspendedStepsThread.Start();
        }

        public async Task CheckSuspendedSteps()
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
                            var steps = await _mediator.Send(new GetEntitiesQuery<Step>
                            {
                                Page = 0,
                                Size = 1000,
                                Expression = (s) => s.Status == StepStatuses.Suspended,
                                Exclusions = new List<Expression<Func<Step, object>>>{
                                        (s) => s.Journal
                                    },
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
        }

        public async Task CheckScheduledExecutions()
        {
            bool printedMessage = false;
            var stopwatch = new Stopwatch();
            ConcurrentDictionary<Guid, DateTime> skipSchedules = new ConcurrentDictionary<Guid, DateTime>();
            while (true)
            {
                try
                {
                    List<Guid> enableScheduleList = new List<Guid>();
                    foreach (var skippedSchedule in skipSchedules)
                    {
                        if (skippedSchedule.Value < DateTime.UtcNow)
                        {
                            enableScheduleList.Add(skippedSchedule.Key);
                        }
                    }

                    foreach (var es in enableScheduleList)
                    {
                        skipSchedules.TryRemove(es, out _);
                    }

                    //Do not run if it is uninitialized
                    if (ClusterStateService.Initialized && _nodeStateService.Role == ConsensusCore.Domain.Enums.NodeState.Leader)
                    {
                        stopwatch.Restart();
                        var startDate = DateTime.Now;

                        if (!printedMessage)
                        {
                            _logger.LogInformation("Detected I am cluster leader, starting to run schedule up cluster");
                            printedMessage = true;
                        }
                        Guid[] skip = skipSchedules.Select(ss => ss.Key).ToArray();
                        
                        var pageSize = 10;
                        var totalSize = _entitiesRepository.Count<ExecutionSchedule>(e => (e.NextRun == null || e.NextRun < DateTime.Now && e.IsDisabled == false) && !skip.Contains(e.Id));
                        int runTasks = 0;
                        for (var i = 0; i < totalSize / pageSize; i++)
                        {
                            Console.WriteLine("Pulling " + (i * pageSize + 1) + "-" + (i * pageSize + pageSize));
                            var executionSchedules = await _entitiesRepository.GetAsync<ExecutionSchedule>(e => (e.NextRun == null || e.NextRun < DateTime.Now && e.IsDisabled == false) && !skip.Contains(e.Id), null, null, pageSize, i);

                            var tasks = executionSchedules.Select((es) => Task.Run(async () =>
                            {
                                try
                                {
                                    await _mediator.Send(new RecalculateExecutionScheduleCommand()
                                    {
                                        Name = es.Name
                                    });
                                    _logger.LogDebug("Executing schedule " + es.Name + " last run " + es.NextRun.ToString("o") + " current run " + DateTime.Now.ToString("o"));

                                    await _mediator.Send(new ExecuteExecutionTemplateCommand()
                                    {
                                        Name = es.ExecutionTemplateName,
                                        ExecutionScheduleId = es.Id,
                                        CreatedBy = SystemUsers.SCHEDULE_MANAGER
                                    });
                                }
                                catch (Exception e)
                                {
                                    _logger.LogError("Failed to run schedule with exception " + e.Message + " skipping the schedule for 5 minutes" + Environment.NewLine + e.StackTrace);
                                    skipSchedules.TryAdd(es.Id, DateTime.UtcNow.AddMinutes(5));
                                }
                                Interlocked.Increment(ref runTasks);
                            }));

                            await Task.WhenAll(tasks);
                        }

                        var totalTime = stopwatch.ElapsedMilliseconds;

                        _metricManagementService.EnqueueTick(new MetricTick()
                        {
                            MetricId = 0,
                            Date = DateTime.Now,
                            Value = totalTime
                        });

                        if (TimeSpan.FromMilliseconds(totalTime) > TimeSpan.FromSeconds(5))
                        {
                            _logger.LogWarning("Scheduler took longer then 5 second to complete a loop, submitted " + runTasks + " steps, took " + (totalTime) + " milliseconds.");
                        }

                        _metricManagementService.EnqueueTick(new MetricTick()
                        {
                            MetricId = (int)MetricIds.SchedulerLatencyMs,
                            Date = startDate,
                            Value = totalTime
                        });

                        if (totalTime < 1000)
                        {
                            Thread.Sleep(1000 - Convert.ToInt32(totalTime));
                        }
                    }
                    else
                    {
                        printedMessage = false;
                        Thread.Sleep(3000);
                    }
                }
                catch (Exception e)
                {

                }
            }
        }

        public async void CollectMetricsEventHandler(object args)
        {
            var currentDateTime = DateTime.Now;
            if (lastSecond != currentDateTime.Second && currentDateTime.Second % secondsOfMetrics == 0)
            {
                lastSecond = currentDateTime.Second;
                var truncatedTime = currentDateTime;
                truncatedTime = truncatedTime.AddTicks(-(truncatedTime.Ticks % TimeSpan.TicksPerSecond));

                DateTime toDate = truncatedTime;
                DateTime fromDate = truncatedTime.AddSeconds(-secondsOfMetrics);

                if (_nodeStateService.Role == ConsensusCore.Domain.Enums.NodeState.Leader)
                {
                    var stepsCount = (await _entitiesRepository.GetAsync<Step>(s => s.CreatedOn > fromDate && s.CreatedOn <= toDate)).ToList().Count();
                    _metricManagementService.EnqueueTick(new MetricTick()
                    {
                        MetricId = (int)MetricIds.QueuedStepsPerSecond,
                        Date = truncatedTime,
                        Value = stepsCount
                    });
                }

                if (Interlocked.CompareExchange(ref _fetchingDbMetrics, 1, 0) == 0)
                {
                    foreach (var metric in await _databaseMetricsCollector.GetMetricsAsync(_nodeStateService.Id))
                    {
                        _metricManagementService.EnqueueTick(metric);
                    }
                    Interlocked.Decrement(ref _fetchingDbMetrics);
                }
                // Console.WriteLine("Writing metrics from " + fromDate.ToString("o") + " to " + toDate.ToString("o") + " value:" + stepsCount);
            }
            /*
            DateTime toDate = DateTime.Now;
            _metricManagementService.EnqueueTick(new MetricTick()
            {
                MetricId = 0,
                Date = toDate,
                Value = (await _entitiesRepository.GetStepsAsync(toDate.AddSeconds(1), toDate)).ToList().Count()
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
