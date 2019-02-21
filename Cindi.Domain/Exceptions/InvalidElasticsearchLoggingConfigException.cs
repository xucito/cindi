using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions
{
    public class InvalidElasticsearchLoggingConfigException : Exception
    {
        public InvalidElasticsearchLoggingConfigException()
        {
        }

        public InvalidElasticsearchLoggingConfigException(string message)
            : base(message)
        {
        }

        public InvalidElasticsearchLoggingConfigException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
