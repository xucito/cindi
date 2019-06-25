using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Utility;
using Cindi.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
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


    }
}
