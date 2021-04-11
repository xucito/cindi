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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Options;
using Cindi.Application.Results;

namespace Cindi.Application.Tests.Steps.Commands
{
    public class AssignStepCommandHandler_tests
    {
        MemoryCache memCache = new MemoryCache(Microsoft.Extensions.Options.Options.Create(new MemoryCacheOptions()));
        
        Mock<IEntitiesRepository> _entitiesRepositoryMock = new Mock<IEntitiesRepository>();
        Mock<IAssignmentCache> _assignmentCacheMock = new Mock<IAssignmentCache>();
        IStateMachine _stateMachine;

        static CindiClusterOptions cindiClusterOptions = new CindiClusterOptions()
        {
            DefaultSuspensionTimeMs = 0
        };

        public AssignStepCommandHandler_tests()
        {
            _stateMachine = new StateMachine();
            _stateMachine.SetEncryptionKey("GCSPHNKWXHPNELFEACOFIWGGUCVWZLUY");
            /*
            _stateMachineMock.Setup(er => er.EncryptionKey).Returns("GCSPHNKWXHPNELFEACOFIWGGUCVWZLUY");

            _stateMachineMock.Setup(cm => cm.GetSettings).Returns(new ClusterSettings()
            {
            });

            _stateMachineMock.Setup(cm => cm.GetState()).Returns(new CindiClusterState()
            {
            });*/
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

            var newStep = SecretSampleData.StepTemplate.GenerateStep(SecretSampleData.StepTemplate.ReferenceId, "", "", "", new Dictionary<string, object>() {
                {"secret", testPhrase}
            }, null, null, _stateMachine.EncryptionKey);

            _assignmentCacheMock.Setup(sr => sr.GetStepTemplate(It.IsAny<string>())).Returns(SecretSampleData.StepTemplate);

            _assignmentCacheMock.Setup(st => st.GetNext(It.IsAny<string[]>())).Returns(newStep);

            var testKey = SecurityUtility.GenerateRSAKeyPair();

            _entitiesRepositoryMock.Setup(kr => kr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<BotKey, bool>>>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var mockLogger = new Mock<ILogger<AssignStepCommandHandler>>();

            var handler = new AssignStepCommandHandler(_stateMachine, mockLogger.Object, memCache, _assignmentCacheMock.Object, _entitiesRepositoryMock.Object);

            var result = await handler.Handle(new AssignStepCommand()
            {
            }, new System.Threading.CancellationToken());

            var convertedResult = result;

            var assignedStep = result.Result;

            Assert.NotEqual(testPhrase, (string)result.Result.Inputs["secret"]);

            //var encryptionTestResult = SecurityUtility.RsaEncryptWithPublic(testPhrase, testKey.PublicKey);
            //Randomized padding is used
            Assert.NotEqual(testPhrase, (string)result.Result.Inputs["secret"]);
            //Decryption using private key should work
            //Assert.Equal(SecurityUtility.SymmetricallyDecrypt(encryptionTestResult, testKey.PrivateKey), 
             Assert.Equal(testPhrase,SecurityUtility.SymmetricallyDecrypt((string)result.Result.Inputs["secret"], SecurityUtility.RsaDecryptWithPrivate(convertedResult.EncryptionKey, testKey.PrivateKey)));
        }


        [Fact]
        public async void EvaluateSecretValueByValueReference()
        {
            
            _assignmentCacheMock.Setup(sr => sr.GetStepTemplate(It.IsAny<string>())).Returns(SecretSampleData.StepTemplate);
            var testPhrase = "This is a test phrase";

            var newStep = SecretSampleData.StepTemplate.GenerateStep(SecretSampleData.StepTemplate.ReferenceId, "", "", "", new Dictionary<string, object>() {
                {"secret", "$secret"}
            }, null, null, _stateMachine.EncryptionKey);

            _assignmentCacheMock.Setup(st => st.GetNext(It.IsAny<string[]>())).Returns(newStep );

            var testKey = SecurityUtility.GenerateRSAKeyPair();


            _entitiesRepositoryMock.Setup(kr => kr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<BotKey, bool>>>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var mockLogger = new Mock<ILogger<AssignStepCommandHandler>>();


            _entitiesRepositoryMock.Setup(s => s.GetFirstOrDefaultAsync<GlobalValue>(It.IsAny<Expression<Func<GlobalValue, bool>>>())).Returns(
                Task.FromResult(new GlobalValue()
                {
                    Name = "secret",
                    Type = InputDataTypes.Secret,
                    Value = SecurityUtility.SymmetricallyEncrypt(testPhrase, _stateMachine.EncryptionKey),
                    Status = GlobalValueStatuses.Enabled,
                    Id = Guid.NewGuid()
                }));
            var handler = new AssignStepCommandHandler(_stateMachine, mockLogger.Object, memCache, _assignmentCacheMock.Object, _entitiesRepositoryMock.Object);

            var result = await handler.Handle(new AssignStepCommand()
            {
            }, new System.Threading.CancellationToken());

            Assert.NotEqual(testPhrase, (string)result.Result.Inputs["secret"]);
            //var encryptionTestResult = SecurityUtility.RsaEncryptWithPublic(testPhrase, testKey.PublicKey);
            //Assert.NotEqual(encryptionTestResult, (string)result.Result.Inputs["secret"]);
            Assert.Equal(testPhrase, SecurityUtility.SymmetricallyDecrypt((string)result.Result.Inputs["secret"], SecurityUtility.RsaDecryptWithPrivate(result.EncryptionKey, testKey.PrivateKey)));
        }


        [Fact]
        public async void EvaluateSecretValueByReference()
        {
            _assignmentCacheMock.Setup(sr => sr.GetStepTemplate(It.IsAny<string>())).Returns(SecretSampleData.StepTemplate);
            var testPhrase = "This is a test phrase";
            var stepTemplate = SecretSampleData.StepTemplate;
            var newStep = stepTemplate.GenerateStep(stepTemplate.ReferenceId, "", "", "", new Dictionary<string, object>() {
                {"secret", "$$secret"}
            }, null, null, _stateMachine.EncryptionKey);

            _assignmentCacheMock.Setup(st => st.GetNext(It.IsAny<string[]>())).Returns(newStep);

            var testKey = SecurityUtility.GenerateRSAKeyPair();


            _entitiesRepositoryMock.Setup(kr => kr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<BotKey, bool>>>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var mockLogger = new Mock<ILogger<AssignStepCommandHandler>>();


            _entitiesRepositoryMock.Setup(s => s.GetFirstOrDefaultAsync<GlobalValue>(It.IsAny<Expression<Func<GlobalValue, bool>>>())).Returns(
                Task.FromResult(new GlobalValue()
                {
                    Name = "secret",
                    Type = InputDataTypes.Secret,
                    Value = SecurityUtility.SymmetricallyEncrypt(testPhrase, _stateMachine.EncryptionKey),
                    Status = GlobalValueStatuses.Enabled,
                    Id = Guid.NewGuid()
                })
                );

            var handler = new AssignStepCommandHandler(_stateMachine, mockLogger.Object, memCache, _assignmentCacheMock.Object, _entitiesRepositoryMock.Object);

            var result = await handler.Handle(new AssignStepCommand()
            {
            }, new System.Threading.CancellationToken());

            Assert.NotEqual(testPhrase, (string)result.Result.Inputs["secret"]);
            //var encryptionTestResult = SecurityUtility.RsaEncryptWithPublic(testPhrase, testKey.PublicKey);
            //Assert.NotEqual(encryptionTestResult, (string)result.Result.Inputs["secret"]);
            Assert.Equal(testPhrase, SecurityUtility.SymmetricallyDecrypt((string)result.Result.Inputs["secret"], SecurityUtility.RsaDecryptWithPrivate(result.EncryptionKey, testKey.PrivateKey)));
        }

        [Fact]
        public async void IgnoreEscapedReferenceSymbol()
        {
            

            _assignmentCacheMock.Setup(sr => sr.GetStepTemplate(It.IsAny<string>())).Returns(SecretSampleData.StepTemplate);
            var testPhrase = "$secret";
            var stepTemplate = SecretSampleData.StepTemplate;
            var newStep = stepTemplate.GenerateStep(stepTemplate.ReferenceId, "", "", "", new Dictionary<string, object>() {
                {"secret", "\\$secret"}
            }, null, null, _stateMachine.EncryptionKey);




            _assignmentCacheMock.Setup(st => st.GetNext(It.IsAny<string[]>())).Returns(newStep);

            var testKey = SecurityUtility.GenerateRSAKeyPair();


            _entitiesRepositoryMock.Setup(kr => kr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<BotKey, bool>>>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var mockLogger = new Mock<ILogger<AssignStepCommandHandler>>();


            _entitiesRepositoryMock.Setup(s => s.GetFirstOrDefaultAsync<GlobalValue>(It.IsAny<Expression<Func<GlobalValue, bool>>>())).Returns(
                Task.FromResult(new GlobalValue()
                {
                    Name = "secret",
                    Type = InputDataTypes.Secret,
                    Value = SecurityUtility.SymmetricallyEncrypt(testPhrase, _stateMachine.EncryptionKey),
                    Status = GlobalValueStatuses.Enabled,
                    Id = Guid.NewGuid()
                })
                );

            var handler = new AssignStepCommandHandler(_stateMachine, mockLogger.Object, memCache, _assignmentCacheMock.Object, _entitiesRepositoryMock.Object);

            var result = await handler.Handle(new AssignStepCommand()
            {
            }, new System.Threading.CancellationToken());

            Assert.NotEqual(testPhrase, (string)result.Result.Inputs["secret"]);
            //var encryptionTestResult = SecurityUtility.RsaEncryptWithPublic(testPhrase, testKey.PublicKey);
            //Assert.NotEqual(encryptionTestResult, (string)result.Result.Inputs["secret"]);
            Assert.Equal(testPhrase, SecurityUtility.SymmetricallyDecrypt((string)result.Result.Inputs["secret"], SecurityUtility.RsaDecryptWithPrivate(result.EncryptionKey, testKey.PrivateKey)));
        }

        [Fact]
        public async void EvaluateIntValueByValueReference()
        {
            _assignmentCacheMock.Setup(sr => sr.GetStepTemplate(It.IsAny<string>())).Returns(FibonacciSampleData.StepTemplate);
            var newStep = FibonacciSampleData.StepTemplate.GenerateStep(FibonacciSampleData.StepTemplate.ReferenceId, "", "", "", new Dictionary<string, object>() {
                {"n-1", "$1"},
                {"n-2", 2 }
            }, null, null, _stateMachine.EncryptionKey);

            _assignmentCacheMock.Setup(st => st.GetNext(It.IsAny<string[]>())).Returns(newStep );

            var testKey = SecurityUtility.GenerateRSAKeyPair();


            _entitiesRepositoryMock.Setup(kr => kr.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<BotKey, bool>>>())).Returns(Task.FromResult(new BotKey()
            {
                PublicEncryptionKey = testKey.PublicKey
            }));

            var mockLogger = new Mock<ILogger<AssignStepCommandHandler>>();


            _entitiesRepositoryMock.Setup(s => s.GetFirstOrDefaultAsync<GlobalValue>(It.IsAny<Expression<Func<GlobalValue, bool>>>())).Returns(
                Task.FromResult(new GlobalValue()
                {
                    Name = "1",
                    Type = InputDataTypes.Int,
                    Value = 1,
                    Status = GlobalValueStatuses.Enabled
                }));

            var handler = new AssignStepCommandHandler(_stateMachine, mockLogger.Object, memCache, _assignmentCacheMock.Object, _entitiesRepositoryMock.Object);

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
