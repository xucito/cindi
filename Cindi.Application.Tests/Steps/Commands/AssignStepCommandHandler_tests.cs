using Cindi.Application.Interfaces;
using Cindi.Application.Options;
using Cindi.Application.Services.ClusterState;
using Cindi.Application.Steps.Commands.AssignStep;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Enums;
using Cindi.Domain.Utilities;
using Cindi.Test.Global;
using Cindi.Test.Global.TestData;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.RPCs;
using ConsensusCore.Domain.RPCs.Shard;
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

        Mock<IEntitiesRepository> entitiesRepository = new Mock<IEntitiesRepository>();

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

            clusterMoq.Setup(cm => cm.GetSettings).Returns(new ClusterSettings()
            {
            });

            clusterMoq.Setup(cm => cm.GetState()).Returns(new CindiClusterState()
            {
            });
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
            var testPhrase = "This is a test";

            Mock<IEntitiesRepository> entitiesRepository = new Mock<IEntitiesRepository>();
            var newStep = SecretSampleData.StepTemplate.GenerateStep(SecretSampleData.StepTemplate.ReferenceId, "", "", "", new Dictionary<string, object>() {
                {"secret", testPhrase}
            }, null, null, ClusterStateService.GetEncryptionKey());

            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(SecretSampleData.StepTemplate));

            entitiesRepository.Setup(st => st.GetAsync(It.IsAny<Expression<Func<Step, bool>>>(), null, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns(Task.FromResult((IEnumerable<Step>)new List<Step> { newStep }));

            var testKey = SecurityUtility.GenerateRSAKeyPair();

            entitiesRepository.Setup(kr => kr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<BotKey, bool>>>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));



            var mockLogger = new Mock<ILogger<AssignStepCommandHandler>>();

            var node = Utility.GetMockConsensusCoreNode();

            node.Setup(s => s.Handle(It.IsAny<RequestDataShard>())).Returns(Task.FromResult(new RequestDataShardResponse()
            {
                AppliedLocked = true,
                IsSuccessful = true
            }));

            var handler = new AssignStepCommandHandler(entitiesRepository.Object, clusterMoq.Object, mockLogger.Object, node.Object);

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
            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync<StepTemplate>(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(SecretSampleData.StepTemplate));
            var testPhrase = "This is a test phrase";

            var newStep = SecretSampleData.StepTemplate.GenerateStep(SecretSampleData.StepTemplate.ReferenceId, "", "", "", new Dictionary<string, object>() {
                {"secret", "$secret"}
            }, null, null, ClusterStateService.GetEncryptionKey());

            entitiesRepository.Setup(st => st.GetAsync<Step>(It.IsAny<Expression<Func<Step, bool>>>(), null, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns(Task.FromResult((IEnumerable<Step>)new List<Step> { newStep }));

            var testKey = SecurityUtility.GenerateRSAKeyPair();


            entitiesRepository.Setup(kr => kr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<BotKey, bool>>>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var mockLogger = new Mock<ILogger<AssignStepCommandHandler>>();


            entitiesRepository.Setup(s => s.GetFirstOrDefaultAsync<GlobalValue>(It.IsAny<Expression<Func<GlobalValue, bool>>>())).Returns(
                Task.FromResult(new GlobalValue("secret", InputDataTypes.Secret, "", SecurityUtility.SymmetricallyEncrypt(testPhrase, ClusterStateService.GetEncryptionKey()), GlobalValueStatuses.Enabled, Guid.NewGuid(), "admin", DateTime.UtcNow))
                );


            var node = Utility.GetMockConsensusCoreNode();
            node.Setup(s => s.Handle(It.IsAny<RequestDataShard>())).Returns(Task.FromResult(new RequestDataShardResponse() { AppliedLocked = true, IsSuccessful = true }));

            var handler = new AssignStepCommandHandler(entitiesRepository.Object, clusterMoq.Object, mockLogger.Object, node.Object);

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

            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync<StepTemplate>(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(SecretSampleData.StepTemplate));
            var testPhrase = "This is a test phrase";
            var stepTemplate = await entitiesRepository.Object.GetFirstOrDefaultAsync<StepTemplate>(st => st.ReferenceId == SecretSampleData.StepTemplate.ReferenceId);
            var newStep = stepTemplate.GenerateStep(stepTemplate.ReferenceId, "", "", "", new Dictionary<string, object>() {
                {"secret", "$$secret"}
            }, null, null, ClusterStateService.GetEncryptionKey());




            entitiesRepository.Setup(st => st.GetAsync<Step>(It.IsAny<Expression<Func<Step, bool>>>(), null, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns(Task.FromResult((IEnumerable<Step>)new List<Step> { newStep }));

            var testKey = SecurityUtility.GenerateRSAKeyPair();


            entitiesRepository.Setup(kr => kr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<BotKey, bool>>>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var mockLogger = new Mock<ILogger<AssignStepCommandHandler>>();


            entitiesRepository.Setup(s => s.GetFirstOrDefaultAsync<GlobalValue>(It.IsAny<Expression<Func<GlobalValue, bool>>>())).Returns(
                Task.FromResult(new GlobalValue("secret", InputDataTypes.Secret, "", SecurityUtility.SymmetricallyEncrypt(testPhrase, ClusterStateService.GetEncryptionKey()), GlobalValueStatuses.Enabled, Guid.NewGuid(), "admin", DateTime.UtcNow))
                );

            var node = Utility.GetMockConsensusCoreNode();
            node.Setup(s => s.Handle(It.IsAny<RequestDataShard>())).Returns(Task.FromResult(new RequestDataShardResponse() { AppliedLocked = true, IsSuccessful = true }));

            var handler = new AssignStepCommandHandler(entitiesRepository.Object, clusterMoq.Object, mockLogger.Object, node.Object);

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

            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync<StepTemplate>(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(SecretSampleData.StepTemplate));
            var testPhrase = "$secret";
            var stepTemplate = await entitiesRepository.Object.GetFirstOrDefaultAsync<StepTemplate>(st => st.ReferenceId == SecretSampleData.StepTemplate.ReferenceId);
            var newStep = stepTemplate.GenerateStep(stepTemplate.ReferenceId, "", "", "", new Dictionary<string, object>() {
                {"secret", "\\$secret"}
            }, null, null, ClusterStateService.GetEncryptionKey());




            entitiesRepository.Setup(st => st.GetAsync<Step>(It.IsAny<Expression<Func<Step, bool>>>(), null, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns(Task.FromResult((IEnumerable<Step>)new List<Step> { newStep }));

            var testKey = SecurityUtility.GenerateRSAKeyPair();


            entitiesRepository.Setup(kr => kr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<BotKey, bool>>>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var mockLogger = new Mock<ILogger<AssignStepCommandHandler>>();


            entitiesRepository.Setup(s => s.GetFirstOrDefaultAsync<GlobalValue>(It.IsAny<Expression<Func<GlobalValue, bool>>>())).Returns(
                Task.FromResult(new GlobalValue("secret", InputDataTypes.Secret, "", SecurityUtility.SymmetricallyEncrypt(testPhrase, ClusterStateService.GetEncryptionKey()), GlobalValueStatuses.Enabled, Guid.NewGuid(), "admin", DateTime.UtcNow))
                );

            var node = Utility.GetMockConsensusCoreNode();
            node.Setup(s => s.Handle(It.IsAny<RequestDataShard>())).Returns(Task.FromResult(new RequestDataShardResponse() { AppliedLocked = true, IsSuccessful = true }));

            var handler = new AssignStepCommandHandler(entitiesRepository.Object, clusterMoq.Object, mockLogger.Object, node.Object);

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

            entitiesRepository.Setup(sr => sr.GetFirstOrDefaultAsync<StepTemplate>(It.IsAny<Expression<Func<StepTemplate, bool>>>())).Returns(Task.FromResult(FibonacciSampleData.StepTemplate));
            var newStep = FibonacciSampleData.StepTemplate.GenerateStep(FibonacciSampleData.StepTemplate.ReferenceId, "", "", "", new Dictionary<string, object>() {
                {"n-1", "$1"},
                {"n-2", 2 }
            }, null, null, ClusterStateService.GetEncryptionKey());




            entitiesRepository.Setup(st => st.GetAsync<Step>(It.IsAny<Expression<Func<Step, bool>>>(), null, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns(Task.FromResult((IEnumerable<Step>)new List<Step> { newStep }));

            var testKey = SecurityUtility.GenerateRSAKeyPair();


            entitiesRepository.Setup(kr => kr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<BotKey, bool>>>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var mockLogger = new Mock<ILogger<AssignStepCommandHandler>>();


            entitiesRepository.Setup(s => s.GetFirstOrDefaultAsync<GlobalValue>(It.IsAny<Expression<Func<GlobalValue, bool>>>())).Returns(
                Task.FromResult(new GlobalValue("1", InputDataTypes.Int, "", 1, GlobalValueStatuses.Enabled, Guid.NewGuid(), "admin", DateTime.UtcNow))
                );

            var node = Utility.GetMockConsensusCoreNode();

            node.Setup(s => s.Handle(It.IsAny<RequestDataShard>())).Returns(Task.FromResult(new RequestDataShardResponse()
            {
                AppliedLocked = true,
                IsSuccessful = true
            }));

            var handler = new AssignStepCommandHandler(entitiesRepository.Object, clusterMoq.Object, mockLogger.Object, node.Object);

            var result = await handler.Handle(new AssignStepCommand()
            {
            }, new System.Threading.CancellationToken());

            Assert.Equal(1, (int)result.Result.Inputs["n-1"]);
            Assert.Equal(2, (int)result.Result.Inputs["n-2"]);
        }

        [Fact]
        public async void DoNotAssignToDisabledBotKey()
        {
            Assert.True(false);
        }
    }
}
