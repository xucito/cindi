using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Enums;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using System;
using Xunit;

namespace Cindi.Domain.Tests
{
    /* 
   public class UnitTest1
   {  [Fact]
       public void EncryptSteps()
       {
           Step step = new Step(new Entities.JournalEntries.Journal(new Entities.JournalEntries.JournalEntry()
           {
               Updates = new System.Collections.Generic.List<Update>()
               {
                   new Update()
                   {
                       FieldName = "inputs",
                       Value = new System.Collections.Generic.Dictionary<string, object>()
               {
                   {   "secret", "This is a secret" }
               },
                       Type = UpdateType.Create
                   }
               }
           }) {

           })
           {
           };
           var key = SecurityUtility.GenerateRSAKeyPair(1024);

           step.EncryptStepSecrets(Enums.EncryptionProtocol.RSA,
               new Entities.StepTemplates.StepTemplate(new Entities.JournalEntries.Journal(
                   new Entities.JournalEntries.JournalEntry()
                   {
                       Updates = new System.Collections.Generic.List<ValueObjects.Update>()
                       {
                           new ValueObjects.Update()
                           {
                               FieldName = "inputdefinitions",
                               Type = UpdateType.Create,
                               Value = new System.Collections.Generic.Dictionary<string, ValueObjects.DynamicDataDescription>()
                   {
                       { "secret", new ValueObjects.DynamicDataDescription()
                       {
                           Type = InputDataTypes.Secret
                       } }
                   }
                           }
                       }
                   }
                   )),
               key.PublicKey
               );

           Assert.NotEqual("This is a secret", step.Inputs["secret"]);

           step.DecryptStepSecrets(Enums.EncryptionProtocol.RSA,
               new Entities.StepTemplates.StepTemplate(new Entities.JournalEntries.Journal(
                   new Entities.JournalEntries.JournalEntry()
                   {
                       Updates = new System.Collections.Generic.List<ValueObjects.Update>()
                       {
                           new ValueObjects.Update()
                           {
                               FieldName = "inputdefinitions",
                               Type = UpdateType.Create,
                               Value = new System.Collections.Generic.Dictionary<string, ValueObjects.DynamicDataDescription>()
                   {
                       { "secret", new ValueObjects.DynamicDataDescription()
                       {
                           Type = InputDataTypes.Secret
                       } }
                   }
                           }
                       }
                   }
                   )),
               key.PrivateKey,
               false
               );
           Assert.Equal("This is a secret", step.Inputs["secret"]);
       }
   }*/
}
