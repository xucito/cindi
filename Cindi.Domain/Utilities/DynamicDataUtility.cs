using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Global;
using Cindi.Domain.Exceptions.Utility;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Cindi.Domain.Utilities
{
    public static class DynamicDataUtility
    {
        public static KeyValuePair<string, Object> GetData(Dictionary<string, object> data, string keyName)
        {
            var result = data.Where(d => d.Key.ToLower() == keyName.ToLower()).ToList();

            if (result.Count() == 0)
            {
                throw new MissingInputException("Missing " + keyName);
            }
            else if (result.Count() > 1)
            {
                throw new DuplicateInputException();
            }
            else
            {
                return result.First();
            }
        }


        public static Dictionary<string, object> DecryptDynamicData(Dictionary<string, DynamicDataDescription> template, Dictionary<string, object> inputs, EncryptionProtocol protocol, string encryptionKey, bool usePublicKey = true)
        {
            if (inputs == null)
            {
                return null;
            }

            Dictionary<string, object> decryptedData = new Dictionary<string, object>();
            var toLoweredDefinitions = template.ToDictionary(entry => entry.Key.ToLower(),
                entry => entry.Value);
            foreach (var input in inputs)
            {
                if (toLoweredDefinitions[input.Key.ToLower()].Type == InputDataTypes.Secret && !InputDataUtility.IsInputReference(input, out _, out _))
                {
                    switch (protocol)
                    {
                        case EncryptionProtocol.AES256:
                            decryptedData.Add(input.Key.ToLower(), SecurityUtility.SymmetricallyDecrypt((string)input.Value, encryptionKey));
                            break;
                        case EncryptionProtocol.RSA:
                            if (usePublicKey)
                            {
                                decryptedData.Add(input.Key.ToLower(), SecurityUtility.RsaDecryptWithPublic(((JsonElement)input.Value).GetString(), encryptionKey));
                            }
                            else
                            {
                                decryptedData.Add(input.Key.ToLower(), SecurityUtility.RsaDecryptWithPrivate((string)input.Value, encryptionKey));
                            }
                            break;
                        default:
                            throw new InvalidEncryptionProtocolException();
                    }
                }
                else
                {
                    decryptedData.Add(input.Key.ToLower(), input.Value);
                }
            }

            return decryptedData;
        }

        public static Dictionary<string, object> EncryptDynamicData(Dictionary<string, DynamicDataDescription> template, Dictionary<string, object> inputs, EncryptionProtocol protocol, string encryptionKey, bool usePublicKey = true)
        {
            if (inputs == null)
            {
                return null;
            }

            Dictionary<string, object> decryptedData = new Dictionary<string, object>();
            var toLoweredDefinitions = template.ToDictionary(entry => entry.Key.ToLower(),
    entry => entry.Value);
            foreach (var input in inputs)
            {
                if (toLoweredDefinitions[input.Key.ToLower()].Type == InputDataTypes.Secret)//&& !InputDataUtility.IsInputReference(input, out _, out _))
                {
                    switch (protocol)
                    {
                        case EncryptionProtocol.AES256:
                            decryptedData.Add(input.Key, SecurityUtility.SymmetricallyEncrypt((string)input.Value, encryptionKey));
                            break;
                        case EncryptionProtocol.RSA:
                            if (usePublicKey)
                            {
                                decryptedData.Add(input.Key, SecurityUtility.RsaEncryptWithPublic((string)input.Value, encryptionKey));
                            }
                            else
                            {
                                decryptedData.Add(input.Key, SecurityUtility.RsaEncryptWithPrivate((string)input.Value, encryptionKey));
                            }
                            break;
                        default:
                            throw new InvalidEncryptionProtocolException();
                    }
                }
                else
                {
                    decryptedData.Add(input.Key, input.Value);
                }
            }

            return decryptedData;
        }
    }

}
