using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Enums
{
    public static class InputDataTypes
    {
        public const string Int = "int";
        public const string String = "string";
        public const string Bool = "bool";
        public const string Object = "object";
        public const string ErrorMessage = "errorMessage";
        public const string Decimal = "decimal";
        public const string DateTime = "dateTime";
        public const string Secret = "secret";

        public static bool IsValidDataType(string dataType)
        {
            if(dataType == Int ||
                dataType == String ||
                dataType == Bool ||
                dataType == Object ||
                dataType == ErrorMessage ||
                dataType == Decimal ||
                dataType == DateTime ||
                dataType == Secret)
            {
                return true; 
            }
            return false; 
        }
    }
}
