using Cindi.Application.StepTemplates.Commands;
using Cindi.Application.StepTemplates.Commands.CreateStepTemplate;
using Cindi.Application.StepTemplates.Queries.GetStepTemplate;
using Cindi.Application.StepTemplates.Queries.GetStepTemplates;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Exceptions;
using Cindi.Presentation.Results;
using Cindi.Presentation.Utility;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Cindi.Presentation.Controllers
{
    public class StepTemplatesController : BaseController
    {

        public StepTemplatesController(ILoggerFactory logger) : base(logger.CreateLogger<StepTemplatesController>())
        {
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]CreateStepTemplateCommand command)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                if(command.OutputDefinitions == null)
                {
                    command.OutputDefinitions = new Dictionary<string, Domain.ValueObjects.DynamicDataDescription>();
                }

                command.CreatedBy = ClaimsUtility.GetId(User);
                var result = await Mediator.Send(command);
                return Ok(new HttpCommandResult<StepTemplate>("/api/steptemplates/" + command.Name + "/" + command.Version, result, null));
            }
            catch (BaseException e)
            {
                Logger.LogError(e.Message);
                stopwatch.Stop();
                return BadRequest(e.ToExceptionResult(stopwatch.ElapsedMilliseconds));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 0, int size = 100)
        {
            return Ok(await Mediator.Send(new GetStepTemplatesQuery()
            {
                Page = 0,
                Size = 100
            }));
        }

        [HttpGet]
        [Route("{name}/{version}")]
        public async Task<IActionResult> Get(string name, string version)
        {
            return Ok(await Mediator.Send(new GetStepTemplateQuery()
            {
                Id = name + ":" + version
            }));
        }
    }
}
