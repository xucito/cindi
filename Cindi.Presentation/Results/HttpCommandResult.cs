using Cindi.Application.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cindi.Presentation.Results
{
    public class HttpCommandResult: CommandResult
    {
        public string HRef { get; set; }

        public HttpCommandResult(string href, long elapsedMs, CommandResult result)
        {
            HRef = href;
            Type = result.Type;
            ElapsedMs = elapsedMs;
            ObjectRefId = result.ObjectRefId;
        }
    }
}
