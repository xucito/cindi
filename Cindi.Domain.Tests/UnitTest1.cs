using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Enums;
using Cindi.Domain.Utilities;
using System;
using Xunit;

namespace Cindi.Domain.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void EncryptSteps()
        {
            Step step = new Step()
            {
                Inputs = new System.Collections.Generic.Dictionary<string, object>()
                {
                    {   "secret", "This is a secret" }
                }
            };
            var key = SecurityUtility.GenerateRSAKeyPair(1024);

            step.EncryptStepSecrets(Enums.EncryptionProtocol.RSA, 
                new Entities.StepTemplates.StepTemplate()
                {
                    InputDefinitions = new System.Collections.Generic.Dictionary<string, ValueObjects.DynamicDataDescription>()
                    {
                        { "secret", new ValueObjects.DynamicDataDescription()
                        {
                            Type = InputDataTypes.Secret
                        } }
                    }
                }, 
                key.PublicKey
                );

            Assert.NotEqual("This is a secret", step.Inputs["secret"]);

            step.DecryptStepSecrets(Enums.EncryptionProtocol.RSA,
                new Entities.StepTemplates.StepTemplate()
                {
                    InputDefinitions = new System.Collections.Generic.Dictionary<string, ValueObjects.DynamicDataDescription>()
                    {
                        { "secret", new ValueObjects.DynamicDataDescription()
                        {
                            Type = InputDataTypes.Secret
                        } }
                    }
                }, 
                key.PrivateKey,
                false
                );
            Assert.Equal("This is a secret", step.Inputs["secret"]);
        }
    }
}
