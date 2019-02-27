using Cindi.Application.Interfaces;
using Cindi.Application.Steps.Commands;
using Cindi.Application.Steps.Commands.CreateStep;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Exceptions.StepTemplates;
using Cindi.Test.Global.MockInterfaces;
using Cindi.Test.Global.TestData;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cindi.Application.Tests.Steps.Commands
{
    public class CreateStepCommandHandler_Tests
    {
        [Fact]
        public async void DetectMissingTemplate()
        {
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();

            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object);

            await Assert.ThrowsAsync<StepTemplateNotFoundException>(async () =>
            {
                await handler.Handle(new CreateStepCommand()
                {
                    StepTemplateId = FibonacciSampleData.StepTemplate.Id
                   
                }, new System.Threading.CancellationToken());
            });
        }

        [Fact]
        public async void DetectCorrectTemplate()
        {
            var TestStep = FibonacciSampleData.Step;
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(FibonacciSampleData.StepTemplate.Id)).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep));

            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object);

            var commandResult = await handler.Handle(new CreateStepCommand()
            {
                StepTemplateId = FibonacciSampleData.StepTemplate.Id,
                Inputs = new Dictionary<string, object>()
                {
                    { "n-1", 1 } ,
                    { "n-2", 2 }
                }
            }, new System.Threading.CancellationToken());

            Assert.NotNull(handler);
            Assert.Equal(TestStep.Id.ToString(), commandResult.ObjectRefId);
        }

        [Fact]
        public async void DetectNoInputs()
        {
            var TestStep = FibonacciSampleData.Step;
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(FibonacciSampleData.StepTemplate.Id)).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep));

            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object);

            await Assert.ThrowsAsync<InvalidStepInputException>(async () => await handler.Handle(new CreateStepCommand()
            {
                StepTemplateId = FibonacciSampleData.StepTemplate.Id
            }, new System.Threading.CancellationToken()));
        }

        [Fact]
        public async void DetectTooManyInputs()
        {
            var TestStep = FibonacciSampleData.Step;
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(FibonacciSampleData.StepTemplate.Id)).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep));

            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object);

            await Assert.ThrowsAsync<InvalidStepInputException>(async () => await handler.Handle(new CreateStepCommand()
            {
                StepTemplateId = FibonacciSampleData.StepTemplate.Id,
                Inputs = new Dictionary<string, object>()
                {
                    { "n-1", 1 } ,
                    { "n-2", 2 },
                    { "n-3", 2 }
                }
            }, new System.Threading.CancellationToken()));
        }

        [Fact]
        public async void DetectMissingInputs()
        {
            var TestStep = FibonacciSampleData.Step;
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(FibonacciSampleData.StepTemplate.Id)).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep));

            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object);

            await Assert.ThrowsAsync<InvalidStepInputException>(async () => await handler.Handle(new CreateStepCommand()
            {
                StepTemplateId = FibonacciSampleData.StepTemplate.Id,
                Inputs = new Dictionary<string, object>()
                {
                    { "n-1", 1 } 
                }
            }, new System.Threading.CancellationToken()));
        }

    }
}
