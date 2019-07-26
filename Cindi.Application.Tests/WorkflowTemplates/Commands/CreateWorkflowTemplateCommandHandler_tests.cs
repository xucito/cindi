using Cindi.Application.Interfaces;
using Cindi.Application.Workflows.Commands;
using Cindi.Application.WorkflowTemplates.Commands.CreateWorkflowTemplate;
using Cindi.Domain.Exceptions.Global;
using Cindi.Domain.Exceptions.StepTemplates;
using Cindi.Test.Global;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Cindi.Test.Global.TestData.FibonacciSampleData;

namespace Cindi.Application.Tests.WorkflowTemplates.Commands
{
    public class CreateWorkflowTemplateCommandHandler_tests
    {
        [Fact]
        public async void DetectMissingStepTemplates()
        {
            FibonacciSequenceData data = new FibonacciSequenceData(5);
            Mock<IWorkflowTemplatesRepository> workflowTemplatesRepository = new Mock<IWorkflowTemplatesRepository>();
            workflowTemplatesRepository.Setup(sr => sr.GetWorkflowTemplateAsync(data.workflowTemplateWithInputs.Id)).Returns(Task.FromResult(data.workflowTemplateWithInputs));
            Mock<IStepTemplatesRepository> stepsRepository = new Mock<IStepTemplatesRepository>();

            var node = Utility.GetMockConsensusCoreNode();

            var handler = new CreateWorkflowTemplateCommandHandler(workflowTemplatesRepository.Object, stepsRepository.Object, node.Object);

            await Assert.ThrowsAsync<StepTemplateNotFoundException>(async () => await handler.Handle(new CreateWorkflowTemplateCommand()
            {
                Name = data.workflowTemplateWithInputs.Name,
                Version = data.workflowTemplateWithInputs.Version,
                InputDefinitions = data.workflowTemplateWithInputs.InputDefinitions,
                LogicBlocks = data.workflowTemplateWithInputs.LogicBlocks
            }, new System.Threading.CancellationToken()));
        }

        [Fact]
        public void DetectMissingSequenceMapping()
        {

        }
    }
}
