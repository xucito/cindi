using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions.SequenceTemplates
{
    public class DuplicateSequenceStepRefException :BaseException
    {
        public DuplicateSequenceStepRefException()
        {
        }

        public DuplicateSequenceStepRefException(string message)
            : base(message)
        {
        }

        public DuplicateSequenceStepRefException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
