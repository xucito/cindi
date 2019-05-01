using Cindi.Application.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cindi.Presentation.Results
{
    public class HttpCommandResult<T> : CommandResult
    {
        public string HRef { get; set; }
        public T Result { get; set; }

        public HttpCommandResult(string href, CommandResult result, T returnObject)
        {
            HRef = href;
            Type = result.Type;
            ElapsedMs = result.ElapsedMs;
            ObjectRefId = result.ObjectRefId;
            Result = returnObject;
        }

        public HttpCommandResult(string href, CommandResult<T> result, T returnObject)
        {
            HRef = href;
            Type = result.Type;
            ElapsedMs = result.ElapsedMs;
            ObjectRefId = result.ObjectRefId;
            Result = returnObject;
        }
    }

    public class HttpCommandResult<Z, T> : HttpCommandResult<T>
    {
        public HttpCommandResult(string href, CommandResult<Z> queryResult, T mappedResponse): base(href, queryResult, mappedResponse)
        {
        }
    }
}
