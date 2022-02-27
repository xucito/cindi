using Cindi.Application.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cindi.Presentation.Results
{
    public class HttpQueryResult<Z, T> : QueryResult<T> where T : class where Z : class
    {
        public HttpQueryResult(QueryResult<Z> queryResult, T mappedResponse)
        {
            this.Count = queryResult.Count;
            this.ElapsedMs = queryResult.ElapsedMs;
            this.Result = mappedResponse;
        }
    }
}
