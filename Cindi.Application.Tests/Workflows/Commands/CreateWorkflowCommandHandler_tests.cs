using Cindi.Application.Interfaces;
using Cindi.Application.Workflows.Commands;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Exceptions.Global;
using Cindi.Test.Global;
using Cindi.Test.Global.TestData;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Node;
using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Cindi.Test.Global.TestData.FibonacciSampleData;
using Cindi.Application.Workflows.Commands.CreateWorkflow;
using Cindi.Domain.Exceptions.Workflows;
using ConsensusCore.Node.Communication.Controllers;
using Cindi.Domain.Entities.WorkflowsTemplates;
using System.Linq.Expressions;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Application.Steps.Commands.CreateStep;
using Cindi.Application.Results;
using Cindi.Domain.Entities.Steps;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Cindi.Application.Tests.Workflows.Commands
{
    public class CreateWorkflowCommandHandler_tests
    {
        Mock<IMediator> _mediator = new Mock<IMediator>();

        Mock<IClusterRequestHandler> _node;

        Mock<ILogger<CreateWorkflowCommandHandler>> _mockStateLogger = new Mock<ILogger<CreateWorkflowCommandHandler>>();

        public CreateWorkflowCommandHandler_tests()
        {
            _node = Utility.GetMockConsensusCoreNode();
        }

        [Fact]
        public async void DetectMissingSequenceTemplate()
        {
            Assert.True(false);
        }

        [Fact]
        public async void PassWorkflowIdToNewStep()
        {
            Assert.True(false);
        }

        [Fact]
        public async void PassWorkflowStepIdToNewStep()
        {
            Assert.True(false);
        }

        [Fact]
        public async void ReturnsSuccessfulOnStepCreationFailure()
        {
            Mock<IEntitiesRepository> entitiesRepository = new Mock<IEntitiesRepository>();
            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<WorkflowTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.ConcurrentWorkflowTemplate));
            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Workflow, bool>>>())).Returns(Task.FromResult(
                    new Workflow(Guid.NewGuid(),
                    FibonacciSampleData.ConcurrentWorkflowTemplate.ReferenceId,
                    new Dictionary<string, object>(),
                    "",
                    "admin",
                    DateTime.Now
                )));

            _mediator.Setup(m => m.Send(It.IsAny<CreateStepCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("FAILED WITH GENERIC ERROR"));

            var handler = new CreateWorkflowCommandHandler(entitiesRepository.Object, _mediator.Object, _node.Object, _mockStateLogger.Object);

            await handler.Handle(new CreateWorkflowCommand()
            {
                WorkflowTemplateId = FibonacciSampleData.ConcurrentWorkflowTemplate.ReferenceId,
                Inputs = new Dictionary<string, object>()
                {
                }
            }, new System.Threading.CancellationToken());
        }

        [Fact]
        public async void ConcurrentStartSteps()
        {
            Mock<IEntitiesRepository> entitiesRepository = new Mock<IEntitiesRepository>();
            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<WorkflowTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.ConcurrentWorkflowTemplate));
            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Workflow, bool>>>())).Returns(Task.FromResult(
                new Workflow(Guid.NewGuid(),
                FibonacciSampleData.ConcurrentWorkflowTemplate.ReferenceId,
                new Dictionary<string, object>(),
                "",
                "admin",
                DateTime.Now
            )));
            var handler = new CreateWorkflowCommandHandler(entitiesRepository.Object, _mediator.Object, _node.Object, _mockStateLogger.Object);

            await handler.Handle(new CreateWorkflowCommand()
            {
                WorkflowTemplateId = FibonacciSampleData.ConcurrentWorkflowTemplate.ReferenceId,
                Inputs = new Dictionary<string, object>()
                {
                }
            }, new System.Threading.CancellationToken());

            _mediator.Verify(e => e.Send(It.IsAny<CreateStepCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(2), "Should call twice to create both starting steps.");
        }


        [Fact]
        public async void DetectExtraInput()
        {
            FibonacciWorkflowData data = new FibonacciWorkflowData(5);
            Mock<IEntitiesRepository> entitiesRepository = new Mock<IEntitiesRepository>();
            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<WorkflowTemplate, bool>>>())).Returns(Task.FromResult(data.workflowTemplateWithInputs));

            var handler = new CreateWorkflowCommandHandler(entitiesRepository.Object, _mediator.Object, _node.Object, _mockStateLogger.Object);

            await Assert.ThrowsAsync<InvalidInputsException>(async () => await handler.Handle(new CreateWorkflowCommand()
            {
                WorkflowTemplateId = data.workflowTemplate.ReferenceId,
                Inputs = new Dictionary<string, object>() {
                    { "n-1", 1 },
                    { "n-2", 1 },
                    { "n-3", 1 }
                }
            }, new System.Threading.CancellationToken()));
        }

        [Fact]
        public async void DetectMissingInput()
        {
            FibonacciWorkflowData data = new FibonacciWorkflowData(5);
            Mock<IEntitiesRepository> entitiesRepository = new Mock<IEntitiesRepository>();
            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<WorkflowTemplate, bool>>>())).Returns(Task.FromResult(data.workflowTemplateWithInputs));
            var handler = new CreateWorkflowCommandHandler(entitiesRepository.Object, _mediator.Object, _node.Object, _mockStateLogger.Object);

            await Assert.ThrowsAsync<MissingInputException>(async () => await handler.Handle(new CreateWorkflowCommand()
            {
                WorkflowTemplateId = data.workflowTemplate.ReferenceId,
                Inputs = new Dictionary<string, object>() { { "n-1", 1 } }
            }, new System.Threading.CancellationToken()));
        }
    }
}
