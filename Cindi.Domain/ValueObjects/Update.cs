using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.ValueObjects
{
    public class Update
    {
        public string Type { get; set; }
        public string FieldName { get; set; }
        public object Value { get; set; }
    }

    public static class UpdateType
    {
        // This value should not have any pre-existing record
        public const string Create = "create";
        // Update existing data
        public const string Override = "override";
        // Append data to a record
        public const string Append = "append";
    }
}
