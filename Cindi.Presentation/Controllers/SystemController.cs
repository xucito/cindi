using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cindi.Application.InternalBots.InternalSteps;
using Cindi.Application.Results;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Presentation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Cindi.Presentation.Controllers
{
    public class SystemController : BaseController
    {
        public SystemController(ILoggerFactory logger) : base(logger.CreateLogger<WorkflowsController>())
        {
        }

        [HttpGet("step-templates")]
        public async Task<IActionResult> GetSystemStepTemplates()
        {
            var stopwatch = new Stopwatch();
            return Ok(new QueryResult<List<StepTemplate>>() {
                Count = InternalStepLibrary.All.Count(),
                Result = InternalStepLibrary.All,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                IsSuccessful = true
            });
        }

        [HttpGet("workflow-templates")]
        public async Task<IActionResult> GetSystemWorkflowTemplates()
        {
            var stopwatch = new Stopwatch();
            return Ok(new QueryResult<List<StepTemplate>>()
            {
                Count = InternalStepLibrary.All.Count(),
                Result = InternalStepLibrary.All,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                IsSuccessful = true
            });
        }
    }
}
