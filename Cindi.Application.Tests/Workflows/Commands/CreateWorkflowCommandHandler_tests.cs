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

namespace Cindi.Application.Tests.Workflows.Commands
{
    public class CreateWorkflowCommandHandler_tests
    {
        Mock<IMediator> _mediator = new Mock<IMediator>();

        Mock<IConsensusCoreNode<CindiClusterState, IBaseRepository>> _node;

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
        public async void DetectExtraInput()
        {
            FibonacciWorkflowData data = new FibonacciWorkflowData(5);
            Mock<IWorkflowsRepository> workflowsRepository = new Mock<IWorkflowsRepository>();
            Mock<IWorkflowTemplatesRepository> workflowTemplatesRepository = new Mock<IWorkflowTemplatesRepository>();
            workflowTemplatesRepository.Setup(sr => sr.GetWorkflowTemplateAsync(data.workflowTemplateWithInputs.ReferenceId)).Returns(Task.FromResult(data.workflowTemplateWithInputs));
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();

            var handler = new CreateWorkflowCommandHandler(workflowsRepository.Object, workflowTemplatesRepository.Object, stepsRepository.Object, stepTemplatesRepository.Object, _mediator.Object, _node.Object);

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
            Mock<IWorkflowsRepository> workflowsRepository = new Mock<IWorkflowsRepository>();
            Mock<IWorkflowTemplatesRepository> workflowTemplatesRepository = new Mock<IWorkflowTemplatesRepository>();
            workflowTemplatesRepository.Setup(sr => sr.GetWorkflowTemplateAsync(data.workflowTemplateWithInputs.ReferenceId)).Returns(Task.FromResult(data.workflowTemplateWithInputs));
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            var handler = new CreateWorkflowCommandHandler(workflowsRepository.Object, workflowTemplatesRepository.Object, stepsRepository.Object, stepTemplatesRepository.Object, _mediator.Object, _node.Object);

            await Assert.ThrowsAsync<MissingInputException>(async () => await handler.Handle(new CreateWorkflowCommand()
            {
                WorkflowTemplateId = data.workflowTemplate.ReferenceId,
                Inputs = new Dictionary<string, object>() { { "n-1", 1 } }
            }, new System.Threading.CancellationToken()));
        }
    }
}
