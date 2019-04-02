using Cindi.Domain.Exceptions.Utility;
using Cindi.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Cindi.Domain.Tests.Utilities
{
    public class SecurityUtility_Tests
    {
        [Fact]
        public void AsymetricalEncryption()
        {
            var testString = "THIS IS A TEST STRING";
            var csp = new RSACryptoServiceProvider(2048);
            var privKey = SecurityUtility.ConvertRSAKeyToString(csp.ExportParameters(true));
            var pubKey = SecurityUtility.ConvertRSAKeyToString(csp.ExportParameters(false));
            var encryptedText = SecurityUtility.AsymmetricallyEncryptText(pubKey, testString);
            Assert.NotEqual(testString, encryptedText);
            var unencryptedText = SecurityUtility.AsymmetricallyDecryptText(privKey, encryptedText);
            Assert.Equal(unencryptedText, testString);

            var fakeRSACryptoProvider = new RSACryptoServiceProvider(2048);
            var fakePrivKey = SecurityUtility.ConvertRSAKeyToString(fakeRSACryptoProvider.ExportParameters(true));


            Assert.Throws<InvalidPrivateKeyException>(() => SecurityUtility.AsymmetricallyDecryptText(fakePrivKey, encryptedText));
            Assert.Throws<InvalidPrivateKeyException>(() => SecurityUtility.AsymmetricallyDecryptText("NOT VALID RSA STRING", encryptedText));
        }

        [Fact]
        public void SymmetricallyAES256Encryption()
        {
            var testString = "THIS IS A TEST STRING";
            var testKey = "65AA8CD46274E4BC1B9958FE47FA3E50";
            Assert.Throws<InvalidPrivateKeyException>(() => SecurityUtility.SymmetricallyEncrypt(testString, "blue"));

            var encryptedString = SecurityUtility.SymmetricallyEncrypt(testString, testKey);

            Assert.NotEqual(encryptedString, testString);

            Assert.Equal(SecurityUtility.SymmetricallyDecrypt(encryptedString, testKey), testString);
        }
    }
}
