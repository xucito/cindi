using Cindi.Domain.Exceptions.Utility;
using Cindi.Domain.Utilities;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
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
        public void AsymetricalEncryption1024()
        {
            var pair = SecurityUtility.GenerateRSAKeyPair();
            var badPair = SecurityUtility.GenerateRSAKeyPair();
            var testString = "THIS IS A TEST STRING";

            var encryptedText = SecurityUtility.RsaEncryptWithPublic(testString, pair.PublicKey);
            var decryptedText = SecurityUtility.RsaDecryptWithPrivate(encryptedText, pair.PrivateKey);

            Assert.Equal(testString, decryptedText);
            Assert.NotEqual(encryptedText, testString);
            Assert.Throws<Org.BouncyCastle.Crypto.InvalidCipherTextException>(() => SecurityUtility.RsaDecryptWithPrivate(encryptedText, badPair.PrivateKey));
        }

        [Fact]
        public void AsymetricalEncryption2048()
        {
            var pair = SecurityUtility.GenerateRSAKeyPair(2048);
            var badPair = SecurityUtility.GenerateRSAKeyPair(2048);
            var testString = "THIS IS A TEST STRING";

            var encryptedText = SecurityUtility.RsaEncryptWithPublic(testString, pair.PublicKey);
            var decryptedText = SecurityUtility.RsaDecryptWithPrivate(encryptedText, pair.PrivateKey);

            Assert.Equal(testString, decryptedText);
            Assert.NotEqual(encryptedText, testString);
            Assert.Throws<Org.BouncyCastle.Crypto.InvalidCipherTextException>(() => SecurityUtility.RsaDecryptWithPrivate(encryptedText, badPair.PrivateKey));
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
