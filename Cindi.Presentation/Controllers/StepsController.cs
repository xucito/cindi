using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cindi.Application.Steps.Commands;
using Cindi.Domain.Exceptions;
using Cindi.Presentation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Cindi.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StepsController : BaseController
    {
        public StepsController(ILoggerFactory logger): base(logger.CreateLogger<StepsController>())
        {

        }

       [HttpPost]
        public async Task<IActionResult> Create(CreateStepCommand command)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                var result = await Mediator.Send(command);
                stopwatch.Stop();
                return Ok(new HttpCommandResult("step", stopwatch.ElapsedMilliseconds, result));
            }
            catch (BaseException e)
            {
                Logger.LogError(e.Message);
                stopwatch.Stop();
                return BadRequest(e.ToExceptionResult(stopwatch.ElapsedMilliseconds));
            }
        }
    }
}