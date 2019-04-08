using Cindi.Domain.Exceptions.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cindi.Domain.Utilities
{
    public class DynamicDataUtility
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
    }
}
