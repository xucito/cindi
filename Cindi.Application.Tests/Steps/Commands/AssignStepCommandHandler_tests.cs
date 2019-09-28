using Cindi.Application.Interfaces;
using Cindi.Application.Options;
using Cindi.Application.Services.ClusterState;
using Cindi.Application.Steps.Commands.AssignStep;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Enums;
using Cindi.Domain.Utilities;
using Cindi.Test.Global;
using Cindi.Test.Global.TestData;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.RPCs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cindi.Application.Tests.Steps.Commands
{
    public class AssignStepCommandHandler_tests
    {
        Mock<IServiceScopeFactory> serviceScopeFactory = new Mock<IServiceScopeFactory>();

        Mock<IClusterStateService> clusterMoq = new Mock<IClusterStateService>();

        static CindiClusterOptions cindiClusterOptions = new CindiClusterOptions()
        {
            DefaultSuspensionTimeMs = 0
        };

        public AssignStepCommandHandler_tests()
        {
            ClusterStateService.GetEncryptionKey = () =>
            {
                return "GCSPHNKWXHPNELFEACOFIWGGUCVWZLUY";
            };
        }

        [Fact]
        public async void DetectGlobalValueReference()
        {
            Assert.True(false);
        }

        [Fact]
        public async void InjectGlobalValueReference()
        {
            Assert.True(false);
        }

        [Fact]
        public async void UnencryptSecretGlobalValueReferenceOnAssignment()
        {
            Assert.True(false);
        }

        [Fact]
        public async void FIFOAssignment()
        {
            Assert.True(false);
        }



        [Fact]
        public async void AssignStepWithSecret()
        {
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(SecretSampleData.StepTemplate.ReferenceId)).Returns(Task.FromResult(SecretSampleData.StepTemplate));
            var testPhrase = "This is a test";
            var stepTemplate = await stepTemplatesRepository.Object.GetStepTemplateAsync(SecretSampleData.StepTemplate.ReferenceId);

            var newStep = stepTemplate.GenerateStep(stepTemplate.ReferenceId, "", "", "", new Dictionary<string, object>() {
                {"secret", testPhrase}
            }, null, null, ClusterStateService.GetEncryptionKey());

            clusterMoq.Setup(cm => cm.IsAssignmentEnabled()).Returns(true);

            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            stepsRepository.Setup(st => st.GetStepsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<List<Expression<Func<Step, object>>>>(), It.IsAny<SortOrder>(), It.IsAny<string>())).Returns(Task.FromResult(new List<Step> { newStep }));

            var testKey = SecurityUtility.GenerateRSAKeyPair();

            Mock<IBotKeysRepository> keysRepository = new Mock<IBotKeysRepository>();
            keysRepository.Setup(kr => kr.GetBotKeyAsync(It.IsAny<Guid>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var mockLogger = new Mock<ILogger<AssignStepCommandHandler>>();

            Mock<IGlobalValuesRepository> globalValueRepository = new Mock<IGlobalValuesRepository>();

            var node = Utility.GetMockConsensusCoreNode();
            node.Setup(s => s.Handle(It.IsAny<RequestDataShard>())).Returns(Task.FromResult(new RequestDataShardResponse() { AppliedLocked = true }));

            var handler = new AssignStepCommandHandler(stepsRepository.Object, clusterMoq.Object, stepTemplatesRepository.Object, keysRepository.Object, mockLogger.Object, globalValueRepository.Object, node.Object);

            var result = await handler.Handle(new AssignStepCommand()
            {
            }, new System.Threading.CancellationToken());

            var assignedStep = result.Result;

            Assert.NotEqual(testPhrase, (string)result.Result.Inputs["secret"]);

            var encryptionTestResult = SecurityUtility.RsaEncryptWithPublic(testPhrase, testKey.PublicKey);
            //Randomized padding is used
            Assert.NotEqual(encryptionTestResult, (string)result.Result.Inputs["secret"]);
            //Decryption using private key should work
            Assert.Equal(SecurityUtility.RsaDecryptWithPrivate(encryptionTestResult, testKey.PrivateKey), SecurityUtility.RsaDecryptWithPrivate((string)result.Result.Inputs["secret"], testKey.PrivateKey));
        }


        [Fact]
        public async void EvaluateSecretValueByValueReference()
        {
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(SecretSampleData.StepTemplate.ReferenceId)).Returns(Task.FromResult(SecretSampleData.StepTemplate));
            var testPhrase = "This is a test phrase";
            var stepTemplate = await stepTemplatesRepository.Object.GetStepTemplateAsync(SecretSampleData.StepTemplate.ReferenceId);
            var newStep = stepTemplate.GenerateStep(stepTemplate.ReferenceId, "", "", "", new Dictionary<string, object>() {
                {"secret", "$secret"}
            }, null, null, ClusterStateService.GetEncryptionKey());

            clusterMoq.Setup(cm => cm.IsAssignmentEnabled()).Returns(true);

            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            stepsRepository.Setup(st => st.GetStepsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<List<Expression<Func<Step, object>>>>(), It.IsAny<SortOrder>(), It.IsAny<string>())).Returns(Task.FromResult(new List<Step> { newStep }));

            var testKey = SecurityUtility.GenerateRSAKeyPair();

            Mock<IBotKeysRepository> keysRepository = new Mock<IBotKeysRepository>();
            keysRepository.Setup(kr => kr.GetBotKeyAsync(It.IsAny<Guid>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var mockLogger = new Mock<ILogger<AssignStepCommandHandler>>();

            Mock<IGlobalValuesRepository> globalValueRepository = new Mock<IGlobalValuesRepository>();
            globalValueRepository.Setup(s => s.GetGlobalValueAsync("secret")).Returns(
                Task.FromResult(new GlobalValue("secret", InputDataTypes.Secret, "", SecurityUtility.SymmetricallyEncrypt(testPhrase, ClusterStateService.GetEncryptionKey()), GlobalValueStatuses.Enabled, Guid.NewGuid(), "admin", DateTime.UtcNow))
                );


            var node = Utility.GetMockConsensusCoreNode();
            node.Setup(s => s.Handle(It.IsAny<RequestDataShard>())).Returns(Task.FromResult(new RequestDataShardResponse() { AppliedLocked = true }));

            var handler = new AssignStepCommandHandler(stepsRepository.Object, clusterMoq.Object, stepTemplatesRepository.Object, keysRepository.Object, mockLogger.Object, globalValueRepository.Object, node.Object);

            var result = await handler.Handle(new AssignStepCommand()
            {
            }, new System.Threading.CancellationToken());

            Assert.NotEqual(testPhrase, (string)result.Result.Inputs["secret"]);
            var encryptionTestResult = SecurityUtility.RsaEncryptWithPublic(testPhrase, testKey.PublicKey);
            Assert.NotEqual(encryptionTestResult, (string)result.Result.Inputs["secret"]);
            Assert.Equal(SecurityUtility.RsaDecryptWithPrivate(encryptionTestResult, testKey.PrivateKey), SecurityUtility.RsaDecryptWithPrivate((string)result.Result.Inputs["secret"], testKey.PrivateKey));
        }


        [Fact]
        public async void EvaluateSecretValueByReference()
        {
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(SecretSampleData.StepTemplate.ReferenceId)).Returns(Task.FromResult(SecretSampleData.StepTemplate));
            var testPhrase = "This is a test phrase";
            var stepTemplate = await stepTemplatesRepository.Object.GetStepTemplateAsync(SecretSampleData.StepTemplate.ReferenceId);
            var newStep = stepTemplate.GenerateStep(stepTemplate.ReferenceId, "", "", "", new Dictionary<string, object>() {
                {"secret", "$$secret"}
            }, null, null, ClusterStateService.GetEncryptionKey());

            clusterMoq.Setup(cm => cm.IsAssignmentEnabled()).Returns(true);

            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            stepsRepository.Setup(st => st.GetStepsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<List<Expression<Func<Step, object>>>>(), It.IsAny<SortOrder>(), It.IsAny<string>())).Returns(Task.FromResult(new List<Step> { newStep }));

            var testKey = SecurityUtility.GenerateRSAKeyPair();

            Mock<IBotKeysRepository> keysRepository = new Mock<IBotKeysRepository>();
            keysRepository.Setup(kr => kr.GetBotKeyAsync(It.IsAny<Guid>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var mockLogger = new Mock<ILogger<AssignStepCommandHandler>>();

            Mock<IGlobalValuesRepository> globalValueRepository = new Mock<IGlobalValuesRepository>();
            globalValueRepository.Setup(s => s.GetGlobalValueAsync("secret")).Returns(
                Task.FromResult(new GlobalValue("secret", InputDataTypes.Secret, "", SecurityUtility.SymmetricallyEncrypt(testPhrase, ClusterStateService.GetEncryptionKey()), GlobalValueStatuses.Enabled, Guid.NewGuid(), "admin", DateTime.UtcNow))
                );

            var node = Utility.GetMockConsensusCoreNode();
            node.Setup(s => s.Handle(It.IsAny<RequestDataShard>())).Returns(Task.FromResult(new RequestDataShardResponse() { AppliedLocked = true }));

            var handler = new AssignStepCommandHandler(stepsRepository.Object, clusterMoq.Object, stepTemplatesRepository.Object, keysRepository.Object, mockLogger.Object, globalValueRepository.Object, node.Object);

            var result = await handler.Handle(new AssignStepCommand()
            {
            }, new System.Threading.CancellationToken());

            Assert.NotEqual(testPhrase, (string)result.Result.Inputs["secret"]);
            var encryptionTestResult = SecurityUtility.RsaEncryptWithPublic(testPhrase, testKey.PublicKey);
            Assert.NotEqual(encryptionTestResult, (string)result.Result.Inputs["secret"]);
            Assert.Equal(SecurityUtility.RsaDecryptWithPrivate(encryptionTestResult, testKey.PrivateKey), SecurityUtility.RsaDecryptWithPrivate((string)result.Result.Inputs["secret"], testKey.PrivateKey));
        }

        [Fact]
        public async void IgnoreEscapedReferenceSymbol()
        {
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(SecretSampleData.StepTemplate.ReferenceId)).Returns(Task.FromResult(SecretSampleData.StepTemplate));
            var testPhrase = "$secret";
            var stepTemplate = await stepTemplatesRepository.Object.GetStepTemplateAsync(SecretSampleData.StepTemplate.ReferenceId);
            var newStep = stepTemplate.GenerateStep(stepTemplate.ReferenceId, "", "", "", new Dictionary<string, object>() {
                {"secret", "\\$secret"}
            }, null, null, ClusterStateService.GetEncryptionKey());

            clusterMoq.Setup(cm => cm.IsAssignmentEnabled()).Returns(true);

            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            stepsRepository.Setup(st => st.GetStepsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<List<Expression<Func<Step, object>>>>(), It.IsAny<SortOrder>(), It.IsAny<string>())).Returns(Task.FromResult(new List<Step> { newStep }));

            var testKey = SecurityUtility.GenerateRSAKeyPair();

            Mock<IBotKeysRepository> keysRepository = new Mock<IBotKeysRepository>();
            keysRepository.Setup(kr => kr.GetBotKeyAsync(It.IsAny<Guid>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var mockLogger = new Mock<ILogger<AssignStepCommandHandler>>();

            Mock<IGlobalValuesRepository> globalValueRepository = new Mock<IGlobalValuesRepository>();
            globalValueRepository.Setup(s => s.GetGlobalValueAsync("secret")).Returns(
                Task.FromResult(new GlobalValue("secret", InputDataTypes.Secret, "", SecurityUtility.SymmetricallyEncrypt(testPhrase, ClusterStateService.GetEncryptionKey()), GlobalValueStatuses.Enabled, Guid.NewGuid(), "admin", DateTime.UtcNow))
                );

            var node = Utility.GetMockConsensusCoreNode();
            node.Setup(s => s.Handle(It.IsAny<RequestDataShard>())).Returns(Task.FromResult(new RequestDataShardResponse() { AppliedLocked = true }));

            var handler = new AssignStepCommandHandler(stepsRepository.Object, clusterMoq.Object, stepTemplatesRepository.Object, keysRepository.Object, mockLogger.Object, globalValueRepository.Object, node.Object);

            var result = await handler.Handle(new AssignStepCommand()
            {
            }, new System.Threading.CancellationToken());

            Assert.NotEqual(testPhrase, (string)result.Result.Inputs["secret"]);
            var encryptionTestResult = SecurityUtility.RsaEncryptWithPublic(testPhrase, testKey.PublicKey);
            Assert.NotEqual(encryptionTestResult, (string)result.Result.Inputs["secret"]);
            Assert.Equal(SecurityUtility.RsaDecryptWithPrivate(encryptionTestResult, testKey.PrivateKey), SecurityUtility.RsaDecryptWithPrivate((string)result.Result.Inputs["secret"], testKey.PrivateKey));
        }

        [Fact]
        public async void EvaluateIntValueByValueReference()
        {
            Mock<IStepTemplatesRepository> stepTemplatesRepository = new Mock<IStepTemplatesRepository>();
            stepTemplatesRepository.Setup(st => st.GetStepTemplateAsync(FibonacciSampleData.StepTemplate.ReferenceId)).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));

            var stepTemplate = await stepTemplatesRepository.Object.GetStepTemplateAsync(FibonacciSampleData.StepTemplate.ReferenceId);
            var newStep = stepTemplate.GenerateStep(stepTemplate.ReferenceId, "", "", "", new Dictionary<string, object>() {
                {"n-1", "$1"},
                {"n-2", 2 }
            }, null, null, ClusterStateService.GetEncryptionKey());

            clusterMoq.Setup(cm => cm.IsAssignmentEnabled()).Returns(true);

            Mock<IStepsRepository> stepsRepository = new Mock<IStepsRepository>();
            stepsRepository.Setup(st => st.GetStepsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<List<Expression<Func<Step, object>>>>(), It.IsAny<SortOrder>(), It.IsAny<string>())).Returns(Task.FromResult(new List<Step> { newStep }));

            var testKey = SecurityUtility.GenerateRSAKeyPair();

            Mock<IBotKeysRepository> keysRepository = new Mock<IBotKeysRepository>();
            keysRepository.Setup(kr => kr.GetBotKeyAsync(It.IsAny<Guid>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var mockLogger = new Mock<ILogger<AssignStepCommandHandler>>();

            Mock<IGlobalValuesRepository> globalValueRepository = new Mock<IGlobalValuesRepository>();
            globalValueRepository.Setup(s => s.GetGlobalValueAsync("1")).Returns(
                Task.FromResult(new GlobalValue("1", InputDataTypes.Int, "", 1, GlobalValueStatuses.Enabled, Guid.NewGuid(), "admin", DateTime.UtcNow))
                );

            var node = Utility.GetMockConsensusCoreNode();
            node.Setup(s => s.Handle(It.IsAny<RequestDataShard>())).Returns(Task.FromResult(new RequestDataShardResponse() { AppliedLocked = true }));

            var handler = new AssignStepCommandHandler(stepsRepository.Object, clusterMoq.Object, stepTemplatesRepository.Object, keysRepository.Object, mockLogger.Object, globalValueRepository.Object, node.Object);

            var result = await handler.Handle(new AssignStepCommand()
            {
            }, new System.Threading.CancellationToken());

            Assert.Equal(1, (int)result.Result.Inputs["n-1"]);
            Assert.Equal(2, (int)result.Result.Inputs["n-2"]);
        }
    }
}
