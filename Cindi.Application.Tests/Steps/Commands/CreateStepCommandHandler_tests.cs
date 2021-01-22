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


using Cindi.Domain.Entities.StepTemplates;
using System.Linq.Expressions;


namespace Cindi.Application.Tests.Steps.Commands
{
    public class CreateStepCommandHandler_Tests
    {

        Mock<IServiceScopeFactory> serviceScopeFactory = new Mock<IServiceScopeFactory>();

        Mock<IClusterStateService> clusterMoq = new Mock<IClusterStateService>();

        // Mock<IClusterRequestHandler> clusterService;

        static CindiClusterOptions cindiClusterOptions = new CindiClusterOptions()
        {
            DefaultSuspensionTimeMs = 0
        };

        public CreateStepCommandHandler_Tests()
        {

            ClusterStateService.GetEncryptionKey = () =>
            {
                return "GCSPHNKWXHPNELFEACOFIWGGUCVWZLUY";
            };

            //clusterService = Utility.GetMockConsensusCoreNode();
        }

        [Fact]
        public async void DetectMissingTemplate()
        {
            Mock<IClusterService> clusterService = new Mock<IClusterService>();

            var handler = new CreateStepCommandHandler(clusterMoq.Object, clusterService.Object);

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
            Mock<IClusterService> clusterService = new Mock<IClusterService>();

            clusterService.Setup(sr => sr.GetFirstOrDefaultAsync<StepTemplate>(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            clusterService.Setup(n => n.Handle(It.IsAny<AddShardWriteOperation>())).Returns(Task.FromResult(new AddShardWriteOperationResponse() { IsSuccessful = true }));


            var handler = new CreateStepCommandHandler(clusterMoq.Object, clusterService.Object); ;

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
            Mock<IClusterService> clusterService = new Mock<IClusterService>();

            clusterService.Setup(sr => sr.GetFirstOrDefaultAsync<StepTemplate>(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            clusterService.Setup(n => n.Handle(It.IsAny<AddShardWriteOperation>())).Returns(Task.FromResult(new AddShardWriteOperationResponse() { IsSuccessful = true }));

            var handler = new CreateStepCommandHandler(clusterMoq.Object, clusterService.Object); ;

            await Assert.ThrowsAsync<InvalidStepInputException>(async () => await handler.Handle(new CreateStepCommand()
            {
                StepTemplateId = FibonacciSampleData.StepTemplate.ReferenceId
            }, new System.Threading.CancellationToken()));
        }

        [Fact]
        public async void DetectTooManyInputs()
        {
            var TestStep = FibonacciSampleData.Step;
            Mock<IClusterService> clusterService = new Mock<IClusterService>();

            clusterService.Setup(sr => sr.GetFirstOrDefaultAsync<StepTemplate>(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            clusterService.Setup(n => n.Handle(It.IsAny<AddShardWriteOperation>())).Returns(Task.FromResult(new AddShardWriteOperationResponse() { IsSuccessful = true }));

            var handler = new CreateStepCommandHandler(clusterMoq.Object, clusterService.Object); ;

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
            Mock<IClusterService> clusterService = new Mock<IClusterService>();

            clusterService.Setup(sr => sr.GetFirstOrDefaultAsync<StepTemplate>(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            clusterService.Setup(n => n.Handle(It.IsAny<AddShardWriteOperation>())).Returns(Task.FromResult(new AddShardWriteOperationResponse() { IsSuccessful = true }));
            var handler = new CreateStepCommandHandler(clusterMoq.Object, clusterService.Object); ;

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
            Mock<IClusterService> clusterService = new Mock<IClusterService>();
            clusterService.Setup(sr => sr.GetFirstOrDefaultAsync<StepTemplate>(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(SecretSampleData.StepTemplate));
            var stepTemplate = await clusterService.Object.GetFirstOrDefaultAsync<StepTemplate>(st => st.ReferenceId == SecretSampleData.StepTemplate.ReferenceId);
            var newStep = stepTemplate.GenerateStep(stepTemplate.ReferenceId, "", "", "", new Dictionary<string, object>() {
                {"secret", "This is a test"}
            }, null, null, _stateMachine.EncryptionKey);

            clusterService.Setup(n => n.Handle(It.IsAny<AddShardWriteOperation>())).Returns(Task.FromResult(new AddShardWriteOperationResponse() { IsSuccessful = true }));

            var handler = new CreateStepCommandHandler(clusterMoq.Object, clusterService.Object); ;

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
            Assert.Equal("This is a test", SecurityUtility.SymmetricallyDecrypt((string)step.Result.Inputs["secret"], _stateMachine.EncryptionKey));
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

        [Fact]
        public async void PassExecutionTemplateId()
        {
            Assert.True(false);
        }

        [Fact]
        public async void PassExecutionScheduleId()
        {
            Assert.True(false);
        }
    }
}
