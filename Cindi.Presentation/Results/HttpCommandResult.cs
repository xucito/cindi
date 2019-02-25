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
    }
}
