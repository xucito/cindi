﻿using Cindi.Application.Entities.Queries;
using Cindi.Application.ExecutionSchedules.Commands.RecalculateExecutionSchedule;
using Cindi.Application.ExecutionTemplates.Commands.ExecuteExecutionTemplate;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterOperation;
using Cindi.Application.Services.ClusterState;
using Cindi.Application.Steps.Commands.UnassignStep;
using Cindi.Application.Workflows.Commands.ScanWorkflow;
using Cindi.Domain.Entities.ExecutionSchedule;
using Cindi.Domain.Entities.Metrics;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Enums;
using Cindi.Domain.Utilities;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Domain.SystemCommands;
using ConsensusCore.Node;
using ConsensusCore.Node.Communication.Controllers;
using ConsensusCore.Node.Services.Raft;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        Thread checkScheduledExecutions;
        Thread getSystemMetrics;
        Thread dataCleanupThread;
        private async Task<double> GetCpuUsageForProcess()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            await Task.Delay(500);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            return cpuUsageTotal * 100;
        }

        private ILogger<ClusterMonitorService> _logger;
        private IClusterRequestHandler node;
        private IEntitiesRepository _entitiesRepository;
        private MetricManagementService _metricManagementService;
        int leaderMonitoringInterval = Timeout.Infinite;
        int nodeMonitoringInterval = Timeout.Infinite;
        int lastSecond = 0;
        // private IDatabaseMetricsCollector _databaseMetricsCollector;
        private NodeStateService _nodeStateService;
        private IOptions<ClusterOptions> _clusterOptions;
        private IClusterStateService _state;

        private int _fetchingDbMetrics = 0; //0 is false, 1 is true

        private Timer monitoringTimer;
        private int secondsOfMetrics = 5;
        IClusterService _clusterService;

        public ClusterMonitorService(
            IServiceProvider sp,
            IClusterRequestHandler _node,
            IEntitiesRepository entitiesRepository,
            MetricManagementService metricManagementService,
            // IDatabaseMetricsCollector databaseMetricsCollector,
            NodeStateService nodeStateService,
            IOptions<ClusterOptions> clusterOptions,
            IClusterService clusterService)
        {
            _clusterService = clusterService;
            _metricManagementService = metricManagementService;
            // var sp = serviceProvider.CreateScope().ServiceProvider;
            _mediator = sp.GetService<IMediator>();
            _logger = sp.GetService<ILogger<ClusterMonitorService>>();
            _state = sp.GetService<IClusterStateService>();

            _logger.LogInformation("Starting clean up service...");
            node = _node;
            _entitiesRepository = entitiesRepository;
            //  _databaseMetricsCollector = databaseMetricsCollector;
            monitoringTimer = new System.Threading.Timer(CollectMetricsEventHandler);
            node.MetricGenerated += metricGenerated;
            _nodeStateService = nodeStateService;
            _clusterOptions = clusterOptions;
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
            monitoringTimer.Change(0, _clusterOptions.Value.MetricsIntervalMs);
            secondsOfMetrics = _clusterOptions.Value.MetricsIntervalMs / 1000;
            checkSuspendedStepsThread = new Thread(async () => await CheckSuspendedSteps());
            checkSuspendedStepsThread.Start();
            checkScheduledExecutions = new Thread(async () => await CheckScheduledExecutions());
            checkScheduledExecutions.Start();
            dataCleanupThread = new Thread(async () => await CleanUpCluster());
            dataCleanupThread.Start();
            getSystemMetrics = new Thread(async () => await GetSystemMetrics());
            getSystemMetrics.Start();
        }

        public async Task GetSystemMetrics()
        {
            while (true)
            {
                var cpuUsage = await GetCpuUsageForProcess();
                _metricManagementService.EnqueueTick(new MetricTick()
                {
                    MetricId = (int)MetricIds.CPUUsagePercent,
                    Date = DateTime.Now,
                    Value = cpuUsage
                });
                await Task.Delay(_clusterOptions.Value.MetricsIntervalMs);
            }
        }

        public async Task CleanUpCluster()
        {
            int rebuildCount = 0;
            while (true)
            {
                if (ClusterStateService.Initialized && _nodeStateService.Role == ConsensusCore.Domain.Enums.NodeState.Leader)
                {
                    var page = 0;
                    long tickPosition = 0;
                    long totalMetricTicks = 0;
                    int cleanedCount = 0;
                    if (_state.GetSettings != null)
                    {
                        DateTime compare = DateTime.Now.AddMilliseconds(-1 * DateTimeMathsUtility.GetMs(_state.GetSettings.MetricRetentionPeriod));
                        var entities = await _entitiesRepository.GetAsync<MetricTick>((s) => s.Date < compare, null, null, 10000);
                        try
                        {
                            foreach (var tick in entities)
                            {
                                var startTime = DateTime.Now;
                                AddShardWriteOperationResponse result = await _clusterService.AddWriteOperation(
                                new EntityWriteOperation<MetricTick>()
                                {
                                    Data = tick,
                                    WaitForSafeWrite = true,
                                    Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Delete,
                                    RemoveLock = false
                                });

                                _logger.LogDebug("Cleanup of record " + tick.Id + " took " + (DateTime.Now - startTime).TotalMilliseconds + " total ticks.");
                                // _logger.LogDebug("Deleted record " + result.ObjectRefId + ".");
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError("Encountered error while trying to delete record " + Environment.NewLine + e.StackTrace);
                        }

                        DateTime stepCompare = DateTime.Now.AddMilliseconds(-1 * DateTimeMathsUtility.GetMs(_state.GetSettings.StepRetentionPeriod));
                        var steps = await _entitiesRepository.GetAsync<Step>((s) => s.CreatedOn < stepCompare && s.Status != StepStatuses.Unassigned, null, null, 10000);

                        try
                        {
                            foreach (var step in steps)
                            {
                                var startTime = DateTime.Now;
                                AddShardWriteOperationResponse result = await _clusterService.AddWriteOperation(
                                new EntityWriteOperation<Step>()
                                {
                                    Data = step,
                                    WaitForSafeWrite = true,
                                    Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Delete,
                                    RemoveLock = false,
                                    User = SystemUsers.CLEANUP_MANAGER
                                });

                                _logger.LogDebug("Cleanup of record " + step.Id + " took " + (DateTime.Now - startTime).TotalMilliseconds + " total ms.");
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError("Encountered error while trying to delete record " + Environment.NewLine + e.StackTrace);
                        }
                    }

                    if (rebuildCount % 10 == 0)
                    {
                        _logger.LogInformation("Rebuilding db.");
                        _entitiesRepository.Rebuild();
                    }
                    rebuildCount++;
                    await CleanupWorkflowsExecutions();
                    await Task.Delay(300000);
                }
            }
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
                                Expression = (s) => s.Status == StepStatuses.Suspended
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
                        await Task.Delay(60000);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Failed to check suspended threads with exception " + e.Message + Environment.NewLine + e.StackTrace);
                    }
                }
                else
                {
                    printedMessage = false;
                    await Task.Delay(60000);
                }
            }
        }

        public async Task<bool> CleanupWorkflowsExecutions()
        {
            _logger.LogDebug("Started clean up of workflow.");
            if (ClusterStateService.Initialized && _nodeStateService.Role == ConsensusCore.Domain.Enums.NodeState.Leader)
            {
                var runningWorkflows = await _entitiesRepository.GetAsync<Workflow>(w => w.Status == WorkflowStatuses.Started && w.CreatedOn < DateTime.Now.AddMinutes(-5));
                foreach (var workflow in runningWorkflows)
                {
                    try
                    {
                        await _mediator.Send(new ScanWorkflowCommand()
                        {
                            WorkflowId = workflow.Id,
                            CreatedBy = SystemUsers.CLEANUP_MANAGER
                        });
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Failed to clean up workflow " + workflow.Id + " with error " + e.Message + Environment.NewLine + e.StackTrace);
                    }

                }
            }
            return true;
        }

        public async Task CheckScheduledExecutions()
        {
            bool printedMessage = false;
            var stopwatch = new Stopwatch();
            ConcurrentDictionary<Guid, DateTime> skipSchedules = new ConcurrentDictionary<Guid, DateTime>();
            var lastLatencyCheck = DateTime.Now;
            long maxLatency = 0;
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


                    // _logger.LogInformation("Starting scheduler loop.");

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
                        _logger.LogDebug("Found " + totalSize + " execution schedules to execute.");
                        for (var i = 0; i < (totalSize + pageSize - 1) / pageSize; i++)
                        {
                            _logger.LogDebug("Pulling " + (i * pageSize + 1) + "-" + (i * pageSize + pageSize));
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


                        if (TimeSpan.FromMilliseconds(totalTime) > TimeSpan.FromSeconds(5))
                        {
                            _logger.LogWarning("Scheduler took longer then 5 second to complete a loop, submitted " + runTasks + " steps, took " + (totalTime) + " milliseconds.");
                        }

                        if (maxLatency < totalTime)
                        {
                            maxLatency = totalTime;
                        }
                        if ((DateTime.Now - lastLatencyCheck).TotalMilliseconds > _clusterOptions.Value.MetricsIntervalMs)
                        {
                            //Console.WriteLine("Collecting total secheduler metrics");
                            _metricManagementService.EnqueueTick(new MetricTick()
                            {
                                MetricId = (int)MetricIds.SchedulerLatencyMs,
                                Date = startDate,
                                Value = totalTime
                            });
                            _logger.LogDebug("Adding a scheduler metric with " + totalTime + " for " + DateTime.Now.ToString("o"));
                            lastLatencyCheck = DateTime.Now;
                            maxLatency = 0;
                        }

                        if (totalTime < 1000)
                        {
                            await Task.Delay(1000 - Convert.ToInt32(totalTime));
                        }
                    }
                    else
                    {
                        printedMessage = false;
                        await Task.Delay(3000);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError("Encountered error " + e.Message + " while trying to complete scheduled execution loop." + Environment.NewLine + e.StackTrace);
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

                /* if (Interlocked.CompareExchange(ref _fetchingDbMetrics, 1, 0) == 0)
                 {
                     foreach (var metric in await _databaseMetricsCollector.GetMetricsAsync(_nodeStateService.Id))
                     {
                         _metricManagementService.EnqueueTick(metric);
                     }
                     Interlocked.Decrement(ref _fetchingDbMetrics);
                 }*/
                _logger.LogDebug("Writing metrics from " + fromDate.ToString("o") + " to " + toDate.ToString("o"));
            }
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
