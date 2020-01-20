using Cindi.Application.Interfaces;
using Cindi.Application.Options;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Application.Steps.Commands;
using Cindi.Application.Steps.Commands.AssignStep;
using Cindi.Application.Steps.Commands.CompleteStep;
using Cindi.Application.Steps.Commands.CreateStep;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.Workflows;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Exceptions.StepTemplates;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using Cindi.Test.Global;
using Cindi.Test.Global.MockInterfaces;
using Cindi.Test.Global.TestData;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Node;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cindi.Application.Tests.Steps.Commands
{
    public class CreateStepCommandHandler_Tests
    {

        Mock<IServiceScopeFactory> serviceScopeFactory = new Mock<IServiceScopeFactory>();

        Mock<IClusterStateService> clusterMoq = new Mock<IClusterStateService>();

        static CindiClusterOptions cindiClusterOptions = new CindiClusterOptions()
        {
            DefaultSuspensionTimeMs  = 0
        };

        Mock<IClusterRequestHandler> node;

        public CreateStepCommandHandler_Tests()
        {

            ClusterStateService.GetEncryptionKey = () =>
            {
                return "GCSPHNKWXHPNELFEACOFIWGGUCVWZLUY";
            };

            node = Utility.GetMockConsensusCoreNode();
        }

        [Fact]
        public async void DetectMissingTemplate()
        {
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();

            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, clusterMoq.Object, node.Object);

            await Assert.ThrowsAsync<StepTemplateNotFoundException>(async () =>
            {
                await handler.Handle(new CreateStepCommand()
                {
                    StepTemplateId = FibonacciSampleData.StepTemplate.ReferenceId

                }, new System.Threading.CancellationToken());
            });
        }

        [Fact]
        public async void DetectCorrectTemplate()
        {
            var TestStep = FibonacciSampleData.Step;
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(FibonacciSampleData.StepTemplate.ReferenceId)).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep));

            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, clusterMoq.Object, node.Object);

            var commandResult = await handler.Handle(new CreateStepCommand()
            {
                StepTemplateId = FibonacciSampleData.StepTemplate.ReferenceId,
                Inputs = new Dictionary<string, object>()
                {
                    { "n-1", 1 } ,
                    { "n-2", 2 }
                }
            }, new System.Threading.CancellationToken());

            Assert.NotNull(handler);
            Assert.Equal(commandResult.Result.Id.ToString(), commandResult.ObjectRefId);
        }

        [Fact]
        public async void DetectNoInputs()
        {
            var TestStep = FibonacciSampleData.Step;
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(FibonacciSampleData.StepTemplate.ReferenceId)).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep));

            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, clusterMoq.Object, node.Object);

            await Assert.ThrowsAsync<InvalidStepInputException>(async () => await handler.Handle(new CreateStepCommand()
            {
                StepTemplateId = FibonacciSampleData.StepTemplate.ReferenceId
            }, new System.Threading.CancellationToken()));
        }

        [Fact]
        public async void DetectTooManyInputs()
        {
            var TestStep = FibonacciSampleData.Step;
            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(FibonacciSampleData.StepTemplate.ReferenceId)).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep));

            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, clusterMoq.Object, node.Object);

            await Assert.ThrowsAsync<InvalidStepInputException>(async () => await handler.Handle(new CreateStepCommand()
            {
                StepTemplateId = FibonacciSampleData.StepTemplate.ReferenceId,
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
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(FibonacciSampleData.StepTemplate.ReferenceId)).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            stepsRepository.Setup(s => s.InsertStepAsync(It.IsAny<Step>())).Returns(Task.FromResult(TestStep));
            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, clusterMoq.Object, node.Object);

            await Assert.ThrowsAsync<InvalidStepInputException>(async () => await handler.Handle(new CreateStepCommand()
            {
                StepTemplateId = FibonacciSampleData.StepTemplate.ReferenceId,
                Inputs = new Dictionary<string, object>()
                {
                    { "n-1", 1 }
                }
            }, new System.Threading.CancellationToken()));
        }


        [Fact]
        public async void CreateStepWithSecret()
        {
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(SecretSampleData.StepTemplate.ReferenceId)).Returns(Task.FromResult(SecretSampleData.StepTemplate));

            var stepTemplate = await stepTemplatesRepository.Object.GetStepTemplateAsync(SecretSampleData.StepTemplate.ReferenceId);
            var newStep = stepTemplate.GenerateStep(stepTemplate.ReferenceId, "", "", "", new Dictionary<string, object>() {
                {"secret", "This is a test"}
            }, null, null, ClusterStateService.GetEncryptionKey());

            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            stepsRepository.Setup(st => st.InsertStepAsync(Moq.It.IsAny<Step>())).Returns(Task.FromResult(newStep));

            var handler = new CreateStepCommandHandler(stepsRepository.Object, stepTemplatesRepository.Object, clusterMoq.Object, node.Object);

            var step = await handler.Handle(new CreateStepCommand()
            {
                StepTemplateId = stepTemplate.ReferenceId,
                Inputs = new Dictionary<string, object>()
                {
                    {"secret", "This is a test" }
                }
            }, new System.Threading.CancellationToken());

            //Test encryption of step worked
            Assert.NotEqual("This is a test", (string)step.Result.Inputs["secret"]);
            //Test decryption of step
            Assert.Equal("This is a test", SecurityUtility.SymmetricallyDecrypt((string)step.Result.Inputs["secret"], ClusterStateService.GetEncryptionKey()));
        }



        [Fact]
        public async void StepIsAlwaysCreatedWithLowerCaseInputs()
        {
            Assert.True(false);
        }

        [Fact]
        public async void StoreReferenceValuesInOriginalGivenFormat()
        {
            Assert.True(false);
        }

        [Fact]
        public async void AssignIdOnCreation()
        {
            Assert.True(false);
        }

        [Fact]
        public async void DontEncryptReferenceSecrets()
        {
            Assert.True(false);
        }
    }
}
