using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Utilities
{
    public static class InputDataUtility
    {
        public static bool IsInputReference(KeyValuePair<string, object> input, out string referenceString, out bool isReferenceByValue)
        {
            isReferenceByValue = false;
            if (input.Value is string && ((string)input.Value).Length > 1)
            {
                string convertedValue = (string)input.Value;
                if (convertedValue[0] == '$')
                {
                    //Copy by reference
                    if (convertedValue[1] == '$')
                    {
                        referenceString = convertedValue.Substring(2, convertedValue.Length - 2);
                    }
                    else
                    {
                        referenceString = convertedValue.Substring(1, convertedValue.Length - 1);
                        isReferenceByValue = true;
                    }
                    return true;
                }
            }

            referenceString = null;
            return false;
        }

        public static Dictionary<string, object> DecryptDynamicData(StepTemplate template, Dictionary<string, object> inputs, EncryptionProtocol protocol, string encryptionKey, bool usePublicKey = true)
        {
            Dictionary<string, object> decryptedData = new Dictionary<string, object>();

            foreach (var input in inputs)
            {
                if (template.InputDefinitions[input.Key].Type == InputDataTypes.Secret && !IsInputReference(input, out _, out _))
                {
                    switch (protocol)
                    {
                        case EncryptionProtocol.AES256:
                            decryptedData.Add(input.Key, SecurityUtility.SymmetricallyDecrypt((string)input.Value, encryptionKey));
                            break;
                        case EncryptionProtocol.RSA:
                            if (usePublicKey)
                            {
                                decryptedData.Add(input.Key, SecurityUtility.RsaDecryptWithPublic((string)input.Value, encryptionKey));
                            }
                            else
                            {
                                decryptedData.Add(input.Key, SecurityUtility.RsaDecryptWithPrivate((string)input.Value, encryptionKey));
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

        public static Dictionary<string, object> EncryptDynamicData(StepTemplate template, Dictionary<string, object> inputs, EncryptionProtocol protocol, string encryptionKey, bool usePublicKey = true)
        {
            Dictionary<string, object> decryptedData = new Dictionary<string, object>();

            foreach (var input in inputs)
            {
                if (template.InputDefinitions[input.Key].Type == InputDataTypes.Secret && !IsInputReference(input, out _, out _))
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
