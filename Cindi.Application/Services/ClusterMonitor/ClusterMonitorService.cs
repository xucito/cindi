using Cindi.Application.Entities.Command.DeleteEntity;
using Cindi.Application.Entities.Queries;
using Cindi.Application.ExecutionSchedules.Commands.RecalculateExecutionSchedule;
using Cindi.Application.ExecutionTemplates.Commands.ExecuteExecutionTemplate;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Application.Steps.Commands.UnassignStep;
using Cindi.Application.Workflows.Commands.ScanWorkflow;
using Cindi.Domain.Entities.ExecutionSchedule;
using Cindi.Domain.Entities.Metrics;
using Cindi.Domain.Entities.Options;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Enums;
using Cindi.Domain.Utilities;
using Cindi.Persistence.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        Thread cleanupWorkflowsExecutions;
        Thread dataCleanupThread;
        private ILogger<ClusterMonitorService> _logger;
        private ApplicationDbContext _context;
        private IMetricTicksRepository _metricTicksRepository;
        private MetricManagementService _metricManagementService;
        int leaderMonitoringInterval = Timeout.Infinite;
        int nodeMonitoringInterval = Timeout.Infinite;
        int lastSecond = 0;
        private IDatabaseMetricsCollector _databaseMetricsCollector;
        private IOptions<ClusterOptions> _clusterOptions;
        private IClusterStateService _state;

        private int _fetchingDbMetrics = 0; //0 is false, 1 is true

        private Timer monitoringTimer;
        private int secondsOfMetrics = 5;

        public ClusterMonitorService(
            IServiceProvider sp,
            ApplicationDbContext context,
            MetricManagementService metricManagementService,
            IMetricTicksRepository metricTicksRepository,
            IDatabaseMetricsCollector databaseMetricsCollector,
            IOptions<ClusterOptions> clusterOptions)
        {
            _metricManagementService = metricManagementService;
            // var sp = serviceProvider.CreateScope().ServiceProvider;
            _mediator = sp.GetService<IMediator>();
            _logger = sp.GetService<ILogger<ClusterMonitorService>>();
            _state = sp.GetService<IClusterStateService>();

            _logger.LogInformation("Starting clean up service...");
            _context = context;
            _metricTicksRepository = metricTicksRepository;
            _databaseMetricsCollector = databaseMetricsCollector;
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
            monitoringTimer.Change(0, 100);
            secondsOfMetrics = _clusterOptions.Value.MetricsIntervalMs / 1000;
            checkSuspendedStepsThread = new Thread(async () => await CheckSuspendedSteps());
            checkSuspendedStepsThread.Start();
            checkScheduledExecutions = new Thread(async () => await CheckScheduledExecutions());
            checkScheduledExecutions.Start();
            dataCleanupThread = new Thread(async () => await CleanUpData());
            dataCleanupThread.Start();
            cleanupWorkflowsExecutions = new Thread(async () => await CleanupWorkflowsExecutions());
            cleanupWorkflowsExecutions.Start();
        }

        public async Task CleanUpData()
        {
            while (true)
            {

                var page = 0;
                long tickPosition = 0;
                long totalMetricTicks = 0;
                int cleanedCount = 0;
                CommandResult result = null;
                do
                {
                    if (_state.GetSettings != null)
                    {
                        var entities = await _context.MetricTicks.Where((s) => s.Date < DateTime.Now.AddMilliseconds(-1 * DateTimeMathsUtility.GetMs(_state.GetSettings.MetricRetentionPeriod))).ToListAsync();
                        try
                        {
                            foreach (var tick in entities)
                            {
                                var startTime = DateTime.Now;
                                result = await _mediator.Send(new DeleteEntityCommand<MetricTick>
                                {
                                    Entity = tick
                                });
                                _logger.LogDebug("Cleanup of record " + result.ObjectRefId + " took " + (DateTime.Now - startTime).TotalMilliseconds + " total ticks.");
                                // _logger.LogDebug("Deleted record " + result.ObjectRefId + ".");
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError("Encountered error while trying to delete record " + Environment.NewLine + e.StackTrace);
                        }

                    }
                    // }
                }
                while (result != null && result.IsSuccessful);
                await Task.Delay(30000);
            }
        }

        public async Task CheckSuspendedSteps()
        {
            bool printedMessage = false;
            while (true)
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
                    await Task.Delay(1000);
                }
                catch (Exception e)
                {
                    _logger.LogError("Failed to check suspended threads with exception " + e.Message + Environment.NewLine + e.StackTrace);
                }
            }
        }

        public async Task CleanupWorkflowsExecutions()
        {
            while (true)
            {
                _logger.LogDebug("Started clean up of workflow.");
                var runningWorkflows = await _context.Workflows.Where(w => !WorkflowStatuses.CompletedStatus.Contains(w.Status) && w.CreatedOn < DateTime.Now.AddMinutes(-5)).ToListAsync();
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
                await Task.Delay(60000);
            }
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

                    stopwatch.Restart();
                    var startDate = DateTime.Now;

                    if (!printedMessage)
                    {
                        _logger.LogInformation("Detected I am cluster leader, starting to run schedule up cluster");
                        printedMessage = true;
                    }
                    Guid[] skip = skipSchedules.Select(ss => ss.Key).ToArray();

                    var pageSize = 10;
                    var totalSize = _context.ExecutionSchedules.Count<ExecutionSchedule>(e => (e.NextRun == null || e.NextRun < DateTime.Now && e.IsDisabled == false) && !skip.Contains(e.Id));
                    int runTasks = 0;
                    _logger.LogDebug("Found " + totalSize + " execution schedules to execute.");
                    for (var i = 0; i < (totalSize + pageSize - 1) / pageSize; i++)
                    {
                        _logger.LogDebug("Pulling " + (i * pageSize + 1) + "-" + (i * pageSize + pageSize));
                        var executionSchedules = await _context.ExecutionSchedules.Where(e => (e.NextRun == null || e.NextRun < DateTime.Now && e.IsDisabled == false) && !skip.Contains(e.Id)).ToListAsync();

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

                    if (maxLatency < totalTime)
                    {
                        maxLatency = totalTime;
                    }
                    if ((DateTime.Now - lastLatencyCheck).TotalMilliseconds > _clusterOptions.Value.MetricsIntervalMs)
                    {
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
                catch (Exception e)
                {
                    _logger.LogError("Encountered error " + e.Message + " while trying to complete scheduled execution loop." + Environment.NewLine + e.StackTrace);
                }
            }
        }
    }
}
