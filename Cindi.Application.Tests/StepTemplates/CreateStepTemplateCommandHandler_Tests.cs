using Cindi.Application.Interfaces;
using Cindi.Application.Services.ClusterOperation;
using Cindi.Application.Services.ClusterState;
using Cindi.Application.StepTemplates.Commands.CreateStepTemplate;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Test.Global;
using Cindi.Test.Global.TestData;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Node;
using ConsensusCore.Node.Communication.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cindi.Application.Tests.StepTemplates
{
    public class CreateStepTemplateCommandHandler_Tests
    {
        Mock<IServiceScopeFactory> serviceScopeFactory = new Mock<IServiceScopeFactory>();

        Mock<IClusterStateService> clusterMoq = new Mock<IClusterStateService>();

        Mock<IClusterRequestHandler> _node;

        public CreateStepTemplateCommandHandler_Tests()
        {

            ClusterStateService.GetEncryptionKey = () =>
            {
                return "GCSPHNKWXHPNELFEACOFIWGGUCVWZLUY";
            };
            _node = Utility.GetMockConsensusCoreNode();
        }

        [Fact]
        public async void DetectDuplicateStepTemplates()
        {
            var TestStep = FibonacciSampleData.Step;
            Mock<ClusterService> clusterService = new Mock<ClusterService>();
            clusterService.Setup(st => st.GetFirstOrDefaultAsync<StepTemplate>(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            var mockStateLogger = new Mock<ILogger<CreateStepTemplateCommandHandler>>();
            var handler = new CreateStepTemplateCommandHandler(clusterService.Object, _node.Object, mockStateLogger.Object);

            var result = await handler.Handle(new CreateStepTemplateCommand()
            {
                InputDefinitions = FibonacciSampleData.StepTemplate.InputDefinitions,
                AllowDynamicInputs = false,
                Name = FibonacciSampleData.StepTemplate.Name,
                Version = FibonacciSampleData.StepTemplate.Version,
                OutputDefinitions = FibonacciSampleData.StepTemplate.OutputDefinitions
            }, new System.Threading.CancellationToken());

            Assert.NotNull(result);

            var matchingResult = await handler.Handle(new CreateStepTemplateCommand()
            {
                InputDefinitions = FibonacciSampleData.StepTemplate.InputDefinitions,
                AllowDynamicInputs = false,
                Name = FibonacciSampleData.StepTemplate.Name,
                Version = FibonacciSampleData.StepTemplate.Version,
                OutputDefinitions = FibonacciSampleData.StepTemplate.OutputDefinitions
            }, new System.Threading.CancellationToken());


            Assert.NotNull(matchingResult);

            //Throws an exception if the input definition doesnt match
            await Assert.ThrowsAnyAsync<Exception>(async () => await handler.Handle(new CreateStepTemplateCommand()
            {
                InputDefinitions = new Dictionary<string, Domain.ValueObjects.DynamicDataDescription>(),
                AllowDynamicInputs = false,
                Name = FibonacciSampleData.StepTemplate.Name,
                Version = FibonacciSampleData.StepTemplate.Version,
                OutputDefinitions = FibonacciSampleData.StepTemplate.OutputDefinitions
            }, new System.Threading.CancellationToken()));

            //Throws an exception if the output definition doesnt match
            await Assert.ThrowsAnyAsync<Exception>(async () => await handler.Handle(new CreateStepTemplateCommand()
            {
                InputDefinitions = FibonacciSampleData.StepTemplate.InputDefinitions,
                AllowDynamicInputs = false,
                Name = FibonacciSampleData.StepTemplate.Name,
                Version = FibonacciSampleData.StepTemplate.Version,
                OutputDefinitions = new Dictionary<string, Domain.ValueObjects.DynamicDataDescription>()
            }, new System.Threading.CancellationToken()));

            //Throws an exception if allow dynamic inputs is different
            await Assert.ThrowsAnyAsync<Exception>(async () => await handler.Handle(new CreateStepTemplateCommand()
            {
                InputDefinitions = FibonacciSampleData.StepTemplate.InputDefinitions,
                AllowDynamicInputs = true,
                Name = FibonacciSampleData.StepTemplate.Name,
                Version = FibonacciSampleData.StepTemplate.Version,
                OutputDefinitions = FibonacciSampleData.StepTemplate.OutputDefinitions
            }, new System.Threading.CancellationToken()));
        }
    }
}
