using Cindi.Application.Interfaces;
using Cindi.Application.Sequences.Commands.CreateSequence;
using Cindi.Application.SequenceTemplates.Commands.CreateSequenceTemplate;
using Cindi.Domain.Exceptions.Global;
using Cindi.Domain.Exceptions.StepTemplates;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Cindi.Test.Global.TestData.FibonacciSampleData;

namespace Cindi.Application.Tests.SequenceTemplates.Commands
{
    public class CreateSequenceTemplateCommandHandler_tests
    {
        [Fact]
        public async void DetectMissingStepTemplates()
        {
            FibonacciSequenceData data = new FibonacciSequenceData(5);
            Mock<ISequenceTemplatesRepository> sequenceTemplatesRepository = new Mock<ISequenceTemplatesRepository>();
            sequenceTemplatesRepository.Setup(sr => sr.GetSequenceTemplateAsync(data.sequenceTemplateWithInputs.Id)).Returns(Task.FromResult(data.sequenceTemplateWithInputs));
            Mock<IStepTemplatesRepository> stepsRepository = new Mock<IStepTemplatesRepository>();

            var handler = new CreateSequenceTemplateCommandHandler(sequenceTemplatesRepository.Object, stepsRepository.Object);

            await Assert.ThrowsAsync<StepTemplateNotFoundException>(async () => await handler.Handle(new CreateSequenceTemplateCommand()
            {
                Name = data.sequenceTemplateWithInputs.Name,
                Version = data.sequenceTemplateWithInputs.Version,
                InputDefinitions = data.sequenceTemplateWithInputs.InputDefinitions,
                LogicBlocks = data.sequenceTemplateWithInputs.LogicBlocks
            }, new System.Threading.CancellationToken()));
        }

        [Fact]
        public void DetectMissingSequenceMapping()
        {

        }
    }
}
