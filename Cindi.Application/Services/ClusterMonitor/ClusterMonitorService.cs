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
using Nest;
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
using Cindi.Domain.Entities.Locks;

namespace Cindi.Application.Services.ClusterMonitor
{
    public class ClusterMonitorService
    {
        private IMediator _mediator;
        Thread checkSuspendedStepsThread;
        Thread checkScheduledExecutions;
        Thread cleanupWorkflowsExecutions;
        Thread dataCleanupLocks;
        Thread dataCleanupThread;
        private ILogger<ClusterMonitorService> _logger;
        private ElasticClient _context;
       // private IMetricTicksRepository _metricTicksRepository;
       // private MetricManagementService _metricManagementService;
       // private IDatabaseMetricsCollector _databaseMetricsCollector;
        private IOptions<ClusterOptions> _clusterOptions;
        private IClusterStateService _state;

        private int _fetchingDbMetrics = 0; //0 is false, 1 is true

        //private Timer monitoringTimer;
        private int secondsOfMetrics = 5;

        public ClusterMonitorService(
            IServiceProvider sp,
            ElasticClient context,
            //MetricManagementService metricManagementService,
            //IMetricTicksRepository metricTicksRepository,
            //IDatabaseMetricsCollector databaseMetricsCollector,
            IOptions<ClusterOptions> clusterOptions)
        {
            //_metricManagementService = metricManagementService;
            // var sp = serviceProvider.CreateScope().ServiceProvider;
            _mediator = sp.GetService<IMediator>();
            _logger = sp.GetService<ILogger<ClusterMonitorService>>();
            _state = sp.GetService<IClusterStateService>();

            _logger.LogInformation("Starting clean up service...");
            _context = context;
            //_metricTicksRepository = metricTicksRepository;
            //_databaseMetricsCollector = databaseMetricsCollector;
            _clusterOptions = clusterOptions;
            Start();
        }

        public void Start()
        {
            //monitoringTimer.Change(0, 100);
            secondsOfMetrics = _clusterOptions.Value.MetricsIntervalMs / 1000;
            checkSuspendedStepsThread = new Thread(async () => await CheckSuspendedSteps());
            checkSuspendedStepsThread.Start();
            checkScheduledExecutions = new Thread(async () => await CheckScheduledExecutions());
            checkScheduledExecutions.Start();
            dataCleanupLocks = new Thread(async () => await CleanUpData());
            dataCleanupLocks.Start();
            cleanupWorkflowsExecutions = new Thread(async () => await CleanupWorkflowsExecutions());
            cleanupWorkflowsExecutions.Start();
            dataCleanupThread = new Thread(async () => await CleanUpCluster());
            dataCleanupThread.Start();
        }

        public async Task CleanUpData()
        {
            while (true)
            {
                var result = await _context.DeleteByQueryAsync<Lock>(q => q.Query(a => a.DateRange(c => c.Field(f => f.ExpireOn).LessThanOrEquals(DateTime.UtcNow))));
                await Task.Delay(60000);
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
                if (_state.GetSettings != null)
                {
                    try
                    {
                        DateTime stepCompare = DateTime.UtcNow.AddMilliseconds(-1 * DateTimeMathsUtility.GetMs(_state.GetSettings.StepRetentionPeriod));

                        var result = await _context.DeleteByQueryAsync<Step>((s) => s.Query(q => q.DateRange(dr => dr.Field(f => f.CreatedOn).LessThanOrEquals(stepCompare)) &&
                          !q.Term(o => o.Status.Suffix("keyword"), StepStatuses.Unassigned) &&
                          !q.Term(o => o.Status.Suffix("keyword"), StepStatuses.Assigned) &&
                          !q.Term(o => o.Status.Suffix("keyword"), StepStatuses.Suspended)));
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Failed to clean up steps with exception " + e.Message + Environment.NewLine + e.StackTrace);
                    }
                }
                await Task.Delay(_state.GetSettings.CleanupInterval);
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
                            Expression = (e => e.Query(q => q.Term(f => f.Status, StepStatuses.Suspended)))
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
                var runningWorkflows = (await _mediator.Send(new GetEntitiesQuery<Workflow>()
                {
                    Expression = e => e.Query(q => q.Bool(m => m.MustNot(a => a.Terms(t => t.Field(f => f.Status).Terms(WorkflowStatuses.CompletedStatus)))
                    .Must(d => d.DateRange(r => r.Field(a => a.CreatedOn).LessThan(DateTimeOffset.UtcNow.AddMinutes(-5).DateTime))))).Size(10000)
                })).Result;
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
            var lastLatencyCheck = DateTimeOffset.UtcNow;
            long maxLatency = 0;
            while (true)
            {
                try
                {

                    stopwatch.Restart();
                    var startDate = DateTimeOffset.UtcNow;
                    int runTasks = 0;

                    var executionSchedules = (await _context.SearchAsync<ExecutionSchedule>(e => e.Query(q => q.Bool(
                            t => t.Must(
                                x => x.Exists(f => f.Field(fi => fi.NextRun)),
                                x => x.DateRange(da => da.Field(f => f.NextRun).LessThan(DateTime.UtcNow)),
                                x => x.Term(f => f.Field(at => at.IsDisabled).Value(false))
                            ))).Size(10000))).Hits.Select(s => s.Source).ToList();

                    var tasks = executionSchedules.Select((es) => Task.Run(async () =>
                    {
                        try
                        {
                            var result = await _mediator.Send(new RecalculateExecutionScheduleCommand()
                            {
                                Name = es.Name
                            });

                            if (result.IsSuccessful)
                            {
                                _logger.LogDebug("Executing schedule " + es.Name + " last run " + es.NextRun.ToString("o") + " current run " + DateTimeOffset.UtcNow.ToString("o"));

                                await _mediator.Send(new ExecuteExecutionTemplateCommand()
                                {
                                    Name = es.ExecutionTemplateName,
                                    ExecutionScheduleId = es.Id,
                                    CreatedBy = SystemUsers.SCHEDULE_MANAGER
                                });
                            }
                            else
                            {
                                _logger.LogError("Failed to execute schedule excectuion template");
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError("Failed to run schedule with exception " + e.Message + " skipping the schedule for 5 minutes" + Environment.NewLine + e.StackTrace);
                        }
                        Interlocked.Increment(ref runTasks);
                    }));
                    await Task.WhenAll(tasks);

                    var totalTime = stopwatch.ElapsedMilliseconds;

                    /*_metricManagementService.EnqueueTick(new MetricTick()
                    {
                        MetricId = 0,
                        Date = DateTimeOffset.UtcNow,
                        Value = totalTime
                    });*/

                    if (TimeSpan.FromMilliseconds(totalTime) > TimeSpan.FromSeconds(5))
                    {
                        _logger.LogWarning("Scheduler took longer then 5 second to complete a loop, submitted " + runTasks + " steps, took " + (totalTime) + " milliseconds.");
                    }

                    if (maxLatency < totalTime)
                    {
                        maxLatency = totalTime;
                    }
                    if ((DateTimeOffset.UtcNow - lastLatencyCheck).TotalMilliseconds > _clusterOptions.Value.MetricsIntervalMs)
                    {
                     /*   _metricManagementService.EnqueueTick(new MetricTick()
                        {
                            MetricId = (int)MetricIds.SchedulerLatencyMs,
                            Date = startDate,
                            Value = totalTime
                        });*/
                        _logger.LogDebug("Adding a scheduler metric with " + totalTime + " for " + DateTimeOffset.UtcNow.ToString("o"));
                        lastLatencyCheck = DateTimeOffset.UtcNow;
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
