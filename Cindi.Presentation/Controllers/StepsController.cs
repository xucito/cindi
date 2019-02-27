using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cindi.Application.Steps.Commands;
using Cindi.Application.Steps.Commands.AssignStep;
using Cindi.Application.Steps.Commands.CreateStep;
using Cindi.Application.Steps.Queries;
using Cindi.Application.Steps.Queries.GetStep;
using Cindi.Application.Steps.Queries.GetSteps;
using Cindi.Domain.Entities.Steps;
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
        public StepsController(ILoggerFactory logger) : base(logger.CreateLogger<StepsController>())
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
                return Ok(new HttpCommandResult<Step>("step", result, null));
            }
            catch (BaseException e)
            {
                Logger.LogError(e.Message);
                stopwatch.Stop();
                return BadRequest(e.ToExceptionResult(stopwatch.ElapsedMilliseconds));
            }
        }

        [HttpPost]
        [Route("assignments")]
        public async Task<IActionResult> GetNextStep(AssignStepCommand command)
        {
            var result = await Mediator.Send(command);
            if (result.ObjectRefId != "")
            {
                var resolvedStep = (await Mediator.Send(new GetStepQuery() { Id = new Guid(result.ObjectRefId) })).Result;
                return Ok(new HttpCommandResult<Step>("step", result, resolvedStep));
            }
            else
            {
                return Ok(new HttpCommandResult<Step>("", result, null));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 0, int size = 100)
        {
            return Ok(await Mediator.Send(new GetStepsQuery()
            {
                Page = page,
                Size = size
            }));
        }

        [HttpPost]

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await Mediator.Send(new GetStepQuery()
            {
                Id = id
            }));
        }
    }
}