using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.StepTemplates
{
    public class NoValidStartingLogicBlockException :BaseException
    {
        public NoValidStartingLogicBlockException()
        {
        }

        public NoValidStartingLogicBlockException(string message)
            : base(message)
        {
        }

        public NoValidStartingLogicBlockException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
