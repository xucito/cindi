using Cindi.Application.Entities.Queries;
using Cindi.Application.ExecutionSchedules.Commands.RecalculateExecutionSchedule;
using Cindi.Application.ExecutionTemplates.Commands.ExecuteExecutionTemplate;
using Cindi.Application.Interfaces;
using Cindi.Application.Steps.Commands.UnassignStep;
using Cindi.Application.Workflows.Commands.ScanWorkflow;
using Cindi.Domain.Entities.ExecutionSchedule;
using Cindi.Domain.Entities.Metrics;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Enums;
using Cindi.Domain.Utilities;



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
        private IStateMachine _stateMachine;

        Task checkSuspendedStepsThread;
        Task checkScheduledExecutions;
        Task getSystemMetrics;
        Task dataCleanupThread;
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
        private IEntitiesRepository _entitiesRepository;
        private MetricManagementService _metricManagementService;

        private Timer monitoringTimer;

        public ClusterMonitorService(
            IServiceProvider sp,
            IEntitiesRepository entitiesRepository,
            MetricManagementService metricManagementService,
            IStateMachine stateMachine,
            IAssignmentCache assigmentCache)
        {
            _stateMachine = stateMachine;
            _metricManagementService = metricManagementService;
            // var sp = serviceProvider.CreateScope().ServiceProvider;
            _mediator = sp.GetService<IMediator>();
            _logger = sp.GetService<ILogger<ClusterMonitorService>>();
            _logger.LogInformation("Starting clean up service...");
            _entitiesRepository = entitiesRepository;
            //  _databaseMetricsCollector = databaseMetricsCollector;
            monitoringTimer = new System.Threading.Timer(CollectMetricsEventHandler);
            Start();
        }

        public void Start()
        {
            monitoringTimer.Change(0, _stateMachine.GetSettings.MetricsIntervalMs);
            checkSuspendedStepsThread = new Task(async () => await CheckSuspendedSteps());
            checkSuspendedStepsThread.Start();
            checkScheduledExecutions = new Task(async () => await CheckScheduledExecutions());
            checkScheduledExecutions.Start();
            dataCleanupThread = new Task(async () => await CleanUpCluster());
            dataCleanupThread.Start();
            getSystemMetrics = new Task(async () => await GetSystemMetrics());
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
                await Task.Delay(_stateMachine.GetSettings.MetricsIntervalMs);
            }
        }

        public async Task CleanUpCluster()
        {
            int rebuildCount = 0;
            while (true)
            {
                var page = 0;
                long tickPosition = 0;
                long totalMetricTicks = 0;
                int cleanedCount = 0;
                if (_stateMachine.GetSettings != null)
                {
                    try
                    {
                        DateTime compare = DateTime.Now.AddMilliseconds(-1 * DateTimeMathsUtility.GetMs(_stateMachine.GetSettings.MetricRetentionPeriod));
                        await _entitiesRepository.Delete<MetricTick>((s) => s.Date < compare);

                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Failed to clean up metrics with exception " + e.Message + Environment.NewLine + e.StackTrace);
                    }
                    DateTime stepCompare = DateTime.Now.AddMilliseconds(-1 * DateTimeMathsUtility.GetMs(_stateMachine.GetSettings.StepRetentionPeriod));

                    try
                    {
                        await _entitiesRepository.Delete<Step>((s) => s.CreatedOn < stepCompare &&
                      s.Status != StepStatuses.Unassigned &&
                      s.Status != StepStatuses.Assigned &&
                      s.Status != StepStatuses.Suspended);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Failed to clean up steps with exception " + e.Message + Environment.NewLine + e.StackTrace);
                    }
                }

                if (rebuildCount % 10 == 0)
                {
                    _logger.LogInformation("Rebuilding db.");
                    _entitiesRepository.Rebuild();
                }
                rebuildCount++;
                await CleanupWorkflowsExecutions();
                await Task.Delay(_stateMachine.GetSettings.CleanupInterval);
            }
        }

        public async Task CheckSuspendedSteps()
        {
            bool printedMessage = false;
            while (true)
            {
                //Do not run if it is uninitialized
                if (_stateMachine.GetState().Initialized)
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
                    if (_stateMachine.GetState().Initialized)
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
                                    var skipRunningStep = false;
                                    Step step = null;
                                    if (!es.EnableConcurrent)
                                    {
                                        var dateToConsiderFrom = DateTime.UtcNow.AddMilliseconds(-1 * es.TimeoutMs);
                                        var existingStep = await _entitiesRepository.GetAsync<Step>(s => s.ExecutionScheduleId == es.Id && (s.Status == StepStatuses.Unassigned || (s.Status == StepStatuses.Assigned && s.CreatedOn > dateToConsiderFrom)));
                                        skipRunningStep = existingStep.Count() > 0;
                                        step = existingStep.FirstOrDefault();
                                    }

                                    await _mediator.Send(new RecalculateExecutionScheduleCommand()
                                    {
                                        Name = es.Name
                                    });
                                    _logger.LogDebug("Executing schedule " + es.Name + " last run " + es.NextRun.ToString("o") + " current run " + DateTime.Now.ToString("o"));

                                    if (!skipRunningStep)
                                    {
                                        await _mediator.Send(new ExecuteExecutionTemplateCommand()
                                        {
                                            Name = es.ExecutionTemplateName,
                                            ExecutionScheduleId = es.Id,
                                            CreatedBy = SystemUsers.SCHEDULE_MANAGER
                                        });
                                    }
                                    else
                                    {
                                        _logger.LogWarning("Should have executed template " + es.ExecutionTemplateName + " for schedule " + es.Name + " however there is an existing step " + step.Id + " running.");
                                    }
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
                        if ((DateTime.Now - lastLatencyCheck).TotalMilliseconds > _stateMachine.GetSettings.MetricsIntervalMs)
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
            (await _entitiesRepository.GetDatabaseMetrics()).ForEach(e =>
            {
                _metricManagementService.EnqueueTick(e);
            });
        }
    }
}
