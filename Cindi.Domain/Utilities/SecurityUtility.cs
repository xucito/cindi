using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using RSACryptoServiceProviderExtensions;
using Cindi.Domain.Exceptions.Utility;

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
            UnicodeEncoding byteConverter = new UnicodeEncoding();
            byte[] dataToEncrypt = byteConverter.GetBytes(text);
            var publicKey = ConvertStringToRSAKey(pubKeyString);
            byte[] encryptedData;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.ImportParameters(publicKey);
                encryptedData = rsa.Encrypt(dataToEncrypt, false);
            }

            return Convert.ToBase64String(encryptedData);
        }

        public static string ConvertRSAKeyToString(RSAParameters key)
        {
            var sw = new System.IO.StringWriter();
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            xs.Serialize(sw, key);
            return sw.ToString();
        }

        public static RSAParameters ConvertStringToRSAKey(string keyStringRepresentation)
        {
            RSAParameters key;
            {
                var sr = new System.IO.StringReader(keyStringRepresentation);
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                key = (RSAParameters)xs.Deserialize(sr);
            }
            return key;
        }

        public static string AsymmetricallyDecryptText(string privateKeyString, string cypherText)
        {
            byte[] decryptedData; using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    RSAParameters privateKey = ConvertStringToRSAKey(privateKeyString);
                    rsa.ImportParameters(privateKey);
                    var bytesCypherText = Convert.FromBase64String(cypherText);
                    decryptedData = rsa.Decrypt(bytesCypherText, false);
                    UnicodeEncoding byteConverter = new UnicodeEncoding();
                    return byteConverter.GetString(decryptedData);
                }
                catch (Exception e)
                {
                    throw new InvalidPrivateKeyException(e.Message);
                }
            }

        }
    }
}
