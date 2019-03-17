using Cindi.Application.Interfaces;
using Cindi.Application.Sequences.Commands.CreateSequence;
using Cindi.Domain.Entities.Sequences;
using Cindi.Domain.Exceptions.Global;
using Cindi.Test.Global.TestData;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Cindi.Test.Global.TestData.FibonacciSampleData;

namespace Cindi.Application.Tests.Sequences.Commands
{
    public class CreateSequenceCommandHandler_tests
    {
        [Fact]
        public async void DetectMissingSequenceTemplate()
        {

        }

        [Fact]
        public async void DetectExtraInput()
        {
            FibonacciSequenceData data = new FibonacciSequenceData(5);
            Mock<ISequencesRepository> sequencesRepository = new Mock<ISequencesRepository>();
            Mock<ISequenceTemplatesRepository> sequenceTemplatesRepository = new Mock<ISequenceTemplatesRepository>();
            sequenceTemplatesRepository.Setup(sr => sr.GetSequenceTemplateAsync(data.sequenceTemplateWithInputs.Id)).Returns(Task.FromResult(data.sequenceTemplateWithInputs));
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();

            var handler = new CreateSequenceCommandHandler(sequencesRepository.Object, sequenceTemplatesRepository.Object, stepsRepository.Object);

            await handler.Handle(new CreateSequenceCommand()
            {
                SequenceTemplateId = data.sequenceTemplate.Id,
                Inputs = new Dictionary<string, object>() {
                    { "n-1", 1 },
                    { "n-2", 1 },
                    { "n-3", 1 }
                }
            }, new System.Threading.CancellationToken());
        }

        [Fact]
        public async void DetectMissingInput()
        {
            FibonacciSequenceData data = new FibonacciSequenceData(5);
            Mock<ISequencesRepository> sequencesRepository = new Mock<ISequencesRepository>();
            Mock<ISequenceTemplatesRepository> sequenceTemplatesRepository = new Mock<ISequenceTemplatesRepository>();
            sequenceTemplatesRepository.Setup(sr => sr.GetSequenceTemplateAsync(data.sequenceTemplateWithInputs.Id)).Returns(Task.FromResult(data.sequenceTemplateWithInputs));
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();

            var handler = new CreateSequenceCommandHandler(sequencesRepository.Object, sequenceTemplatesRepository.Object, stepsRepository.Object);

            await Assert.ThrowsAsync<MissingInputException>(async () => await handler.Handle(new CreateSequenceCommand()
            {
                SequenceTemplateId = data.sequenceTemplate.Id,
                Inputs = new Dictionary<string, object>() { { "n-1", 1 } }
            }, new System.Threading.CancellationToken()));
        }
    }
}
