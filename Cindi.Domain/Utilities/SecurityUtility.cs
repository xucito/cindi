using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using RSACryptoServiceProviderExtensions;
using Cindi.Domain.Exceptions.Utility;
using System.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;
using Cindi.Domain.ValueObjects;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;
using PemWriter = Org.BouncyCastle.OpenSsl.PemWriter;

namespace Cindi.Domain.Utilities
{
    //https://gist.github.com/doncadavona/fd493b6ced456371da8879c22bb1c263
    public class SecurityUtility
    {
        private static readonly Encoding encoding = Encoding.UTF8;

        public static string SymmetricallyEncrypt(string plainText, string key)
        {
            if (encoding.GetBytes(key).Length != 32)
            {
                throw new InvalidPrivateKeyException("Private key must be 32 bytes.");
            }

            try
            {
                RijndaelManaged aes = new RijndaelManaged();
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;

                aes.Key = encoding.GetBytes(key);
                aes.GenerateIV();

                ICryptoTransform AESEncrypt = aes.CreateEncryptor(aes.Key, aes.IV);
                byte[] buffer = encoding.GetBytes(plainText);

                string encryptedText = Convert.ToBase64String(AESEncrypt.TransformFinalBlock(buffer, 0, buffer.Length));

                String mac = "";

                mac = BitConverter.ToString(HmacSHA256(Convert.ToBase64String(aes.IV) + encryptedText, key)).Replace("-", "").ToLower();

                var keyValues = new Dictionary<string, object>
                {
                    { "iv", Convert.ToBase64String(aes.IV) },
                    { "value", encryptedText },
                    { "mac", mac },
                };

                return Convert.ToBase64String(encoding.GetBytes(JsonConvert.SerializeObject(keyValues)));
            }
            catch (Exception e)
            {
                throw new Exception("Error encrypting: " + e.Message);
            }
        }

        public static byte[] GenerateSalt(int bytes)
        {
            byte[] salt = new byte[bytes / 8];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return salt;
        }

        public static string SymmetricallyDecrypt(string encryptedString, string key)
        {
            try
            {
                if (encryptedString != null)
                {
                    RijndaelManaged aes = new RijndaelManaged();
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Mode = CipherMode.CBC;
                    aes.Key = encoding.GetBytes(key);

                    // Base 64 decode
                    byte[] base64Decoded = Convert.FromBase64String(encryptedString);
                    string base64DecodedStr = encoding.GetString(base64Decoded);

                    // JSON Decode base64Str
                    var payload = JsonConvert.DeserializeObject<Dictionary<string, string>>(base64DecodedStr);

                    aes.IV = Convert.FromBase64String(payload["iv"]);

                    ICryptoTransform AESDecrypt = aes.CreateDecryptor(aes.Key, aes.IV);
                    byte[] buffer = Convert.FromBase64String(payload["value"]);
                    return encoding.GetString(AESDecrypt.TransformFinalBlock(buffer, 0, buffer.Length));
                }
                return null;
            }
            catch (Exception e)
            {
                throw new Exception("Error decrypting: " + e.Message);
            }
        }

        static byte[] HmacSHA256(String data, String key)
        {
            using (HMACSHA256 hmac = new HMACSHA256(encoding.GetBytes(key)))
            {
                return hmac.ComputeHash(encoding.GetBytes(data));
            }
        }

        public static string OneWayHash(string plaintext, byte[] salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                        salt: salt,
                        password: plaintext,
                        prf: KeyDerivationPrf.HMACSHA256,
                        iterationCount: 10000,
                        numBytesRequested: 256 / 8));
        }

        public static bool IsMatchingHash(string plainText, string hash, byte[] salt)
        {
            return OneWayHash(plainText, salt) == hash;
        }

        public static string RandomString(int size, bool forceLowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (forceLowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }

        public static string AsymmetricallyEncryptText(string pubKeyString, string text)
        {
            return RsaEncryptWithPublic(text, pubKeyString);
        }

        public static string AsymmetricallyDecryptText(string privateKeyString, string cypherText)
        {
            return RsaDecryptWithPrivate(cypherText, privateKeyString);
        }

        //https://www.rahulsingla.com/comment/219809
        public static RSAEncodedKeyPair GenerateRSAKeyPair(int strength = 1024)
        {
            RsaKeyPairGenerator g = new RsaKeyPairGenerator();

            g.Init(new KeyGenerationParameters(new SecureRandom(), strength));
            var pair = g.GenerateKeyPair();

            string privateKey = "";
            string publicKey = "";

            using (var sw = new System.IO.StringWriter())
            {
                var pw = new PemWriter(sw);
                pw.WriteObject(pair.Private);
                privateKey = sw.ToString();
            }

            using (var sw = new System.IO.StringWriter())
            {
                var pw = new PemWriter(sw);
                pw.WriteObject(pair.Public);
                publicKey = sw.ToString();
            }

            return new RSAEncodedKeyPair()
            {
                PrivateKey = privateKey,
                PublicKey = publicKey
            };
        }

        //https://stackoverflow.com/questions/28086321/c-sharp-bouncycastle-rsa-encryption-with-public-private-keys
        public static string RsaEncryptWithPublic(string clearText, string publicKey)
        {
            if (clearText == null)
            {
                return null;
            }
            var bytesToEncrypt = Encoding.UTF8.GetBytes(clearText);

            var encryptEngine = new Pkcs1Encoding(new RsaEngine());

            using (var txtreader = new StringReader(publicKey))
            {
                var keyParameter = (AsymmetricKeyParameter)new PemReader(txtreader).ReadObject();

                encryptEngine.Init(true, keyParameter);
            }

            var encrypted = Convert.ToBase64String(encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length));
            return encrypted;
        }

        public static string RsaDecryptWithPrivate(string base64Input, string privateKey)
        {
            if (base64Input == null)
            {
                return null;
            }
            var trimmedKey = privateKey;
            var bytesToDecrypt = Convert.FromBase64String(base64Input);

            AsymmetricCipherKeyPair keyPair;
            var decryptEngine = new Pkcs1Encoding(new RsaEngine());

            using (var txtreader = new StringReader(trimmedKey))
            {
                keyPair = (AsymmetricCipherKeyPair)new PemReader(txtreader).ReadObject();
                decryptEngine.Init(false, keyPair.Private);
            }

            var decrypted = Encoding.UTF8.GetString(decryptEngine.ProcessBlock(bytesToDecrypt, 0, bytesToDecrypt.Length));
            return decrypted;
        }

        public static string RsaDecryptWithPublic(string base64Input, string publicKey)
        {
            if(base64Input == null)
            {
                return null;
            }

            var bytesToDecrypt = Convert.FromBase64String(base64Input);

            var decryptEngine = new Pkcs1Encoding(new RsaEngine());

            using (var txtreader = new StringReader(publicKey))
            {
                var keyParameter = (AsymmetricKeyParameter)new PemReader(txtreader).ReadObject();

                decryptEngine.Init(false, keyParameter);
            }

            var decrypted = Encoding.UTF8.GetString(decryptEngine.ProcessBlock(bytesToDecrypt, 0, bytesToDecrypt.Length));
            return decrypted;
        }

        public static string RsaEncryptWithPrivate(string clearText, string privateKey)
        {
            if (clearText == null)
            {
                return null;
            }

            var bytesToEncrypt = Encoding.UTF8.GetBytes(clearText);

            var encryptEngine = new Pkcs1Encoding(new RsaEngine());

            using (var txtreader = new StringReader(privateKey))
            {
                var keyPair = (AsymmetricCipherKeyPair)new PemReader(txtreader).ReadObject();

                encryptEngine.Init(true, keyPair.Private);
            }

            var encrypted = Convert.ToBase64String(encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length));
            return encrypted;
        }
    }
}
