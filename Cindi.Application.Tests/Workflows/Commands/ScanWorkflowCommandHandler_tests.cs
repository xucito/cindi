﻿using Cindi.Application.Interfaces;
using Cindi.Application.Options;
using Cindi.Application.Services.ClusterState;
using Cindi.Application.Workflows.Commands.ScanWorkflow;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Test.Global;
using Cindi.Test.Global.TestData;
using ConsensusCore.Node.Communication.Controllers;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Cindi.Test.Global.TestData.FibonacciSampleData;

namespace Cindi.Application.Tests.Workflows.Commands
{
    public class ScanWorkflowCommandHandler_tests
    {
        Mock<IMediator> _mediator = new Mock<IMediator>();

        Mock<IClusterRequestHandler> _node;

        Mock<IClusterStateService> clusterMoq = new Mock<IClusterStateService>();

        Mock<IOptionsMonitor<CindiClusterOptions>> _optionsMonitor;

        static CindiClusterOptions cindiClusterOptions = new CindiClusterOptions()
        {
            DefaultSuspensionTimeMs = 0
        };


        public ScanWorkflowCommandHandler_tests()
        {
            _node = Utility.GetMockConsensusCoreNode();
            ClusterStateService.GetEncryptionKey = () =>
            {
                return "GCSPHNKWXHPNELFEACOFIWGGUCVWZLUY";
            };


            _optionsMonitor = new Mock<IOptionsMonitor<CindiClusterOptions>>();
            _optionsMonitor.Setup(o => o.CurrentValue).Returns(cindiClusterOptions);
        }

        [Fact]
        public async void DontActionRunningWorkflow()
        {
            Mock<IEntitiesRepository> entitiesRepository = new Mock<IEntitiesRepository>();

            FibonacciWorkflowData data = new FibonacciWorkflowData(5);

            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Workflow, bool>>>())).Returns(Task.FromResult(new Workflow(Guid.NewGuid(), data.workflowTemplateWithInputs.ReferenceId, new Dictionary<string, object>()
            {
            }, "", "", DateTime.Now)));

            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<WorkflowTemplate, bool>>>())).Returns(Task.FromResult(data.workflowTemplateWithInputs));
            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(data.stepTemplate));
            entitiesRepository.Setup(sr => sr.GetAsync(It.IsAny<Expression<Func<Step, bool>>>(), null, null, 10, 0)).Returns(Task.FromResult((IEnumerable<Step>)new List<Step>() {
                new Step() {
                Status = StepStatuses.Assigned
            } }));
            var mockStateLogger = new Mock<ILogger<ScanWorkflowCommandHandler>>();


            var handler = new ScanWorkflowCommandHandler(entitiesRepository.Object, clusterMoq.Object, mockStateLogger.Object, _optionsMonitor.Object, _mediator.Object, _node.Object);

            var result = await handler.Handle(new ScanWorkflowCommand()
            {
                WorkflowId = Guid.NewGuid()
            }, new System.Threading.CancellationToken());
            Assert.Single(result.Messages);
            Assert.Contains("is running", result.Messages[0]);
        }

        [Fact]
        public async void DetectMissingStartingStep()
        {

        }

        [Fact]
        public async void RerunFailedWorkflowStepCreation()
        {
            Mock<IEntitiesRepository> entitiesRepository = new Mock<IEntitiesRepository>();

            FibonacciWorkflowData data = new FibonacciWorkflowData(5);

            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Workflow, bool>>>())).Returns(Task.FromResult(
                new Workflow(Guid.NewGuid(),
                FibonacciSampleData.ConcurrentWorkflowTemplate.ReferenceId,
                new Dictionary<string, object>(),
                "",
                "admin",
                DateTime.Now
            )));

            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<WorkflowTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.ConcurrentWorkflowTemplate));
            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            entitiesRepository.Setup(sr => sr.GetAsync(It.IsAny<Expression<Func<Step, bool>>>(), null, null, 10, 0)).Returns(Task.FromResult((IEnumerable<Step>)new List<Step>() {
                    new Step()
                        {
                            Status = StepStatuses.Error,
                            Name  = "0"
                        }
                    }));
            var mockStateLogger = new Mock<ILogger<ScanWorkflowCommandHandler>>();

            clusterMoq.Setup(cm => cm.WasLockObtained(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(true);

            var handler = new ScanWorkflowCommandHandler(entitiesRepository.Object, clusterMoq.Object, mockStateLogger.Object, _optionsMonitor.Object, _mediator.Object, _node.Object);

            var result = await handler.Handle(new ScanWorkflowCommand()
            {
                WorkflowId = Guid.NewGuid()
            }, new System.Threading.CancellationToken());

            Assert.Single(result.Messages);
            Assert.Contains("Started workflow step 1", result.Messages[0]);
        }

        [Fact]
        public async void UpdateWorkflowStatusWhenComplete()
        {
            Mock<IEntitiesRepository> entitiesRepository = new Mock<IEntitiesRepository>();

            FibonacciWorkflowData data = new FibonacciWorkflowData(5);

                entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Workflow, bool>>>())).Returns(Task.FromResult(new Workflow(Guid.NewGuid(), data.workflowTemplateWithInputs.ReferenceId, new Dictionary<string, object>()
                    {
                    }, "", "", DateTime.Now)));

                    entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<WorkflowTemplate, bool>>>())).Returns(Task.FromResult(data.workflowTemplateWithInputs));
                    entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(data.stepTemplate));
                    entitiesRepository.Setup(sr => sr.GetAsync(It.IsAny<Expression<Func<Step, bool>>>(), null, null, 10, 0)).Returns(Task.FromResult((IEnumerable<Step>)new List<Step>() {
                    new Step()
                        {
                            Status = StepStatuses.Error,
                            Name  = "0"
                        }
                    }));
                    var mockStateLogger = new Mock<ILogger<ScanWorkflowCommandHandler>>();

            clusterMoq.Setup(cm => cm.WasLockObtained(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(true);

        var handler = new ScanWorkflowCommandHandler(entitiesRepository.Object, clusterMoq.Object, mockStateLogger.Object, _optionsMonitor.Object, _mediator.Object, _node.Object);

        var result = await handler.Handle(new ScanWorkflowCommand()
        {
            WorkflowId = Guid.NewGuid()
        }, new System.Threading.CancellationToken());
        Assert.Single(result.Messages);
            Assert.Contains("Updated workflow status", result.Messages[0]);
        }
    }
}
