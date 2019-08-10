using Cindi.Application.Interfaces;
using Cindi.Application.Services.ClusterState;
using Cindi.Application.Steps.Commands.UnassignStep;
using Cindi.Application.Steps.Queries.GetSteps;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Node;
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
        private IConsensusCoreNode<CindiClusterState, IBaseRepository> node;

        public ClusterMonitorService(IServiceScopeFactory serviceProvider,
            IConsensusCoreNode<CindiClusterState, IBaseRepository> _node)
        {
            var sp = serviceProvider.CreateScope().ServiceProvider;
            _mediator = sp.GetService<IMediator>();
            _logger = sp.GetService<ILogger<ClusterMonitorService>>();

            _logger.LogInformation("Starting clean up service...");
            node = _node;
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
                bool printedMessage = false;
                while (true)
                {
                    //Do not run if it is uninitialized
                    if (ClusterStateService.Initialized && node.IsLeader)
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
                            if (cleanedCount > 0)
                            {
                                _logger.LogInformation("Cleaned " + cleanedCount + " steps");
                            }
                            Thread.Sleep(1000);
                        }
                        catch(Exception e)
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
    }
}
