using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.Global
{
    public class MissingOutputException :BaseException
    {
        public MissingOutputException()
        {
        }

        public MissingOutputException(string message)
            : base(message)
        {
        }

        public MissingOutputException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
