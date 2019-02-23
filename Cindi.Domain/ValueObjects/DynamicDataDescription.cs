using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.ValueObjects
{
    public class DynamicDataDescription
    {
        public string Description { get; set; }
        public int Type { get; set; }

        public enum InputDataType { Int, String, Bool, Object, ErrorMessage, Decimal, DateTime }
    }
}
