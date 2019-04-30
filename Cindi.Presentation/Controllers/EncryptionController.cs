using Cindi.Application.Steps.Queries.UnencryptStepField;
using Cindi.Domain.Exceptions;
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
    public class EncryptionController : BaseController
    {
        public EncryptionController(ILoggerFactory logger) : base(logger.CreateLogger<EncryptionController>())
        {
        }

        [HttpGet("steps/{id}/fields/{fieldName}")]
        public async Task<IActionResult> UnencryptStepSecret(Guid id, string fieldName)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                return Ok(await Mediator.Send(new UnencryptStepFieldQuery()
                {
                    StepId = id,
                    FieldName = fieldName,
                    UserId = ClaimsUtility.GetId(User)
                }));
            }
            catch (BaseException e)
            {
                Logger.LogError(e.Message);
                return BadRequest(e.ToExceptionResult(stopwatch.ElapsedMilliseconds));
            }
        }
    }
}
