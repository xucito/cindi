using Cindi.Application.Interfaces;
using Cindi.Application.Options;

using Cindi.Application.Services.ClusterState;
using Cindi.Application.Workflows.Commands.ScanWorkflow;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Test.Global;
using Cindi.Test.Global.TestData;

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
        Mock<IEntitiesRepository> _entitiesRepositoryMock = new Mock<IEntitiesRepository>();
        Mock<IStateMachine> _stateMachineMock = new Mock<IStateMachine>();

        Mock<IOptionsMonitor<CindiClusterOptions>> _optionsMonitor;

        static CindiClusterOptions cindiClusterOptions = new CindiClusterOptions()
        {
            DefaultSuspensionTimeMs = 0
        };


        public ScanWorkflowCommandHandler_tests()
        {
            _stateMachineMock.Setup(er => er.EncryptionKey).Returns("GCSPHNKWXHPNELFEACOFIWGGUCVWZLUY");
            _optionsMonitor = new Mock<IOptionsMonitor<CindiClusterOptions>>();
            _optionsMonitor.Setup(o => o.CurrentValue).Returns(cindiClusterOptions);
        }

        [Fact]
        public async void DontActionRunningWorkflow()
        {
            FibonacciWorkflowData data = new FibonacciWorkflowData(5);

            _entitiesRepositoryMock.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Workflow, bool>>>())).Returns(Task.FromResult(new Workflow()
            {
                Id = Guid.NewGuid(),
                WorkflowTemplateId = data.workflowTemplateWithInputs.ReferenceId,
                Inputs = new Dictionary<string, object>()
                {
                }
            }));

            _entitiesRepositoryMock.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<WorkflowTemplate, bool>>>())).Returns(Task.FromResult(data.workflowTemplateWithInputs));
            _entitiesRepositoryMock.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(data.stepTemplate));
            _entitiesRepositoryMock.Setup(sr => sr.GetAsync(It.IsAny<Expression<Func<Step, bool>>>(), null, null, 10, 0)).Returns(Task.FromResult((IEnumerable<Step>)new List<Step>() {
                new Step() {
                Status = StepStatuses.Assigned
            } }));
            var mockStateLogger = new Mock<ILogger<ScanWorkflowCommandHandler>>();


            var handler = new ScanWorkflowCommandHandler(_stateMachineMock.Object, mockStateLogger.Object, _optionsMonitor.Object, _mediator.Object, _entitiesRepositoryMock.Object);

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
            Assert.False(true);/*
            FibonacciWorkflowData data = new FibonacciWorkflowData(5);

            _entitiesRepositoryMock.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Workflow, bool>>>())).Returns(Task.FromResult(
                new Workflow()
                {
                    Id = Guid.NewGuid(),
                    WorkflowTemplateId = FibonacciSampleData.ConcurrentWorkflowTemplate.ReferenceId,
                    Inputs = new Dictionary<string, object>()
                }
            ));

            _entitiesRepositoryMock.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<WorkflowTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.ConcurrentWorkflowTemplate));
            _entitiesRepositoryMock.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            _entitiesRepositoryMock.Setup(sr => sr.GetAsync(It.IsAny<Expression<Func<Step, bool>>>(), null, null, 10, 0)).Returns(Task.FromResult((IEnumerable<Step>)new List<Step>() {
                    new Step()
                        {
                            Status = StepStatuses.Error,
                            Name  = "0"
                        }
                    }));
            var mockStateLogger = new Mock<ILogger<ScanWorkflowCommandHandler>>();

            _stateMachineMock.Setup(cm => cm.IsEntityLocked(It.IsAny<Guid>())).Returns(true);

            var handler = new ScanWorkflowCommandHandler(_stateMachineMock.Object, mockStateLogger.Object, _optionsMonitor.Object, _mediator.Object, _entitiesRepositoryMock.Object);

            var result = await handler.Handle(new ScanWorkflowCommand()
            {
                WorkflowId = Guid.NewGuid()
            }, new System.Threading.CancellationToken());

            Assert.Single(result.Messages);
            Assert.Contains("Started workflow step 1", result.Messages[0]);*/
        }

        [Fact]
        public async void UpdateWorkflowStatusWhenComplete()
        {
            Assert.True(false);/*
            FibonacciWorkflowData data = new FibonacciWorkflowData(5);

            _entitiesRepositoryMock.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Workflow, bool>>>())).Returns(Task.FromResult(new Workflow()
            {
                Id = Guid.NewGuid(),
                WorkflowTemplateId = data.workflowTemplateWithInputs.ReferenceId,
                Inputs = new Dictionary<string, object>()
            }));

            _entitiesRepositoryMock.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<WorkflowTemplate, bool>>>())).Returns(Task.FromResult(data.workflowTemplateWithInputs));
            _entitiesRepositoryMock.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(data.stepTemplate));
            _entitiesRepositoryMock.Setup(sr => sr.GetAsync(It.IsAny<Expression<Func<Step, bool>>>(), null, null, 10, 0)).Returns(Task.FromResult((IEnumerable<Step>)new List<Step>() {
                    new Step()
                        {
                            Status = StepStatuses.Error,
                            Name  = "0"
                        }
                    }));
            var mockStateLogger = new Mock<ILogger<ScanWorkflowCommandHandler>>();

            _stateMachineMock.Setup(cm => cm.IsEntityLocked(It.IsAny<Guid>())).Returns(true);

            var handler = new ScanWorkflowCommandHandler(_stateMachineMock.Object, mockStateLogger.Object, _optionsMonitor.Object, _mediator.Object, _entitiesRepositoryMock.Object);

            var result = await handler.Handle(new ScanWorkflowCommand()
            {
                WorkflowId = Guid.NewGuid()
            }, new System.Threading.CancellationToken());
            Assert.Single(result.Messages);
            Assert.Contains("Updated workflow status", result.Messages[0]);*/
        }
    }
}
