using Cindi.Application.Sequences.Commands.CreateSequence;
using Cindi.Application.SequenceTemplates.Commands.CreateSequenceTemplate;
using Cindi.Application.SequenceTemplates.Queries.GetSequenceTemplate;
using Cindi.Application.SequenceTemplates.Queries.GetSequenceTemplates;
using Cindi.Domain.Entities.SequencesTemplates;
using Cindi.Domain.Exceptions;
using Cindi.Persistence;
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
    public class SequenceTemplatesController: BaseController
    {
        public SequenceTemplatesController(ILoggerFactory logger) : base(logger.CreateLogger<SequenceTemplatesController>())
        {
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]CreateSequenceTemplateCommand command)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                command.CreatedBy = ClaimsUtility.GetId(User);
                var result = await Mediator.Send(command);
                return Ok(new HttpCommandResult<SequenceTemplate>("/api/sequenceTemplate/" + command.Name + "/" + command.Version, result, null));
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
            return Ok(await Mediator.Send(new GetSequenceTemplatesQuery()
            {
                Page = 0,
                Size = 100
            }));
        }

        [HttpGet]
        [Route("{name}/{version}")]
        public async Task<IActionResult> Get(string name, string version)
        {
            return Ok(await Mediator.Send(new GetSequenceTemplateQuery()
            {
                Id = name + ":" + version
            }));
        }
    }
}
