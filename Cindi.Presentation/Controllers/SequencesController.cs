using Cindi.Application.Sequences.Commands.CreateSequence;
using Cindi.Application.Sequences.Queries.GetSequence;
using Cindi.Application.Sequences.Queries.GetSequences;
using Cindi.Application.Sequences.Queries.GetSequenceSteps;
using Cindi.Domain.Entities.Sequences;
using Cindi.Domain.Exceptions;
using Cindi.Presentation.Results;
using Cindi.Presentation.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Cindi.Presentation.Controllers
{
    public class SequencesController: BaseController
    {
        public SequencesController(ILoggerFactory logger) : base(logger.CreateLogger<SequencesController>())
        {
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSequenceCommand command)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                command.CreatedBy = ClaimsUtility.GetId(User);
                var result = await Mediator.Send(command);
                return Ok(new HttpCommandResult<Sequence>("step", result, null));
            }
            catch (BaseException e)
            {
                Logger.LogError(e.Message);
                stopwatch.Stop();
                return BadRequest(e.ToExceptionResult(stopwatch.ElapsedMilliseconds));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 0, int size = 100, string status = null)
        {
            return Ok(await Mediator.Send(new GetSequencesQuery()
            {
                Page = page,
                Size = size,
                Status = status
            }));
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await Mediator.Send(new GetSequenceQuery()
            {
                Id = id
            }));
        }

        [HttpGet]
        [Route("{id}/steps")]
        public async Task<IActionResult> GetSequenceSteps(Guid id)
        {
            return Ok(await Mediator.Send(new GetSequenceStepsQuery()
            {
                SequenceId = id
            }));
        }

    }
}
