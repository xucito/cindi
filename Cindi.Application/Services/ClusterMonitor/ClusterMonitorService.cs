﻿using Cindi.Application.Steps.Commands.UnassignStep;
using Cindi.Application.Steps.Queries.GetSteps;
using Cindi.Domain.Entities.Steps;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Cindi.Application.Services.ClusterMonitor
{
    public class ClusterMonitorService
    {
        private IMediator _mediator;
        Thread checkSuspendedStepsThread;
        private ILogger<ClusterMonitorService> _logger;

        public ClusterMonitorService(IServiceScopeFactory serviceProvider)
        {
            var sp = serviceProvider.CreateScope().ServiceProvider;
            _mediator = sp.GetService<IMediator>();
            _logger = sp.GetService<ILogger<ClusterMonitorService>>();
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

            checkSuspendedStepsThread = new Thread(async () =>
            {
                while (true)
                {
                    _logger.LogInformation("Cleaning up suspended steps...");
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
                            if (step.SuspendedUntil < DateTime.UtcNow)
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
                    _logger.LogInformation("Cleaned " + cleanedCount + " steps");
                    Thread.Sleep(1000);
                }
            });

            checkSuspendedStepsThread.Start();
        }
    }
}