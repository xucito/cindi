using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Results
{
    public class QueryResult<T>
    {
        public long Count { get; set; }
        public T Result { get; set; }
        public long ElapsedMs { get; set; }
    }
}
