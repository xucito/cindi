using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Enums
{
    public class Exceptions
    {
        public enum ExceptionCodes {
            AtomicOperationException,
            ConflictingStepTemplateException,
            ConflictingStepTest
        }
    }
}
