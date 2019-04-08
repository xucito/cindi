using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Exceptions
{
    public class ExceptionResult
    {
        public string ExceptionName { get; set; }
        public string Message { get; set; }
        public DateTime Generated { get; set; }
        public long ElapsedMs { get; set; }
    }
}
