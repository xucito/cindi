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

        public HttpCommandResult(string href, CommandResult result)
        {
            HRef = href;
            Type = result.Type;
            ElapsedMs = result.ElapsedMs;
            ObjectRefId = result.ObjectRefId;
        }
    }
}
