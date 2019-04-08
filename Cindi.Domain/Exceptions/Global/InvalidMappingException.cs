using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Global
{
    public class InvalidMappingException :BaseException
    {
        public InvalidMappingException()
        {
        }

        public InvalidMappingException(string message)
            : base(message)
        {
        }

        public InvalidMappingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
