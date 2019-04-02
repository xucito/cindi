using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cindi.Application.Steps.Commands;
using Cindi.Application.Steps.Commands.AppendStepLog;
using Cindi.Application.Steps.Commands.AssignStep;
using Cindi.Application.Steps.Commands.CompleteStep;
using Cindi.Application.Steps.Commands.CreateStep;
using Cindi.Application.Steps.Queries;
using Cindi.Application.Steps.Queries.GetStep;
using Cindi.Application.Steps.Queries.GetSteps;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions;
using Cindi.Domain.Utilities;
using Cindi.Presentation.Results;
using Cindi.Presentation.Utility;
using Cindi.Presentation.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Cindi.Presentation.Controllers
{
    [Authorize]
    public class StepsController : BaseController
    {
        public StepsController(ILoggerFactory logger) : base(logger.CreateLogger<StepsController>())
        {

        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateStepCommand command, bool? wait_for_completion, string timeout = "30s")
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                command.CreatedBy = ClaimsUtility.GetId(User);
                var result = await Mediator.Send(command);

                Step step = (await Mediator.Send(new GetStepQuery()
                {
                    Id = new Guid(result.ObjectRefId)
                })).Result;


                if (wait_for_completion.HasValue && wait_for_completion.Value)
                {
                    var ms = DateTimeMathsUtility.GetMs(timeout);

                    while (!StepStatuses.IsCompleteStatus(step.Status) && stopwatch.ElapsedMilliseconds < ms)
                    {
                        step = (await Mediator.Send(new GetStepQuery()
                        {
                            Id = new Guid(result.ObjectRefId)
                        })).Result;
                    }

                }


                return Ok(new HttpCommandResult<Step>("step", result, step));
            }
            catch (BaseException e)
            {
                Logger.LogError(e.Message);
                stopwatch.Stop();
                return BadRequest(e.ToExceptionResult(stopwatch.ElapsedMilliseconds));
            }
        }

        [HttpPost]
        [Route("assignment-requests")]
        public async Task<IActionResult> GetNextStep(AssignStepCommand command)
        {
            command.Id = ClaimsUtility.GetId(User);
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

        [HttpPost]
        [Route("{id}/logs")]
        public async Task<IActionResult> AddLog(Guid id, AppendStepLogVM command)
        {
            var appendCommand = new AppendStepLogCommand()
            {
                StepId = id,
                Log = command.Log,
                CreatedBy = ClaimsUtility.GetId(User)
            };
            var result = await Mediator.Send(appendCommand);
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

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> CompleteAssignment(Guid id, CompleteStepVM commandVM)
        {
            var completeStepCommand = new CompleteStepCommand()
            {
                Id = id,
                Status = commandVM.Status.ToLower(),
                StatusCode = commandVM.StatusCode,
                Log = commandVM.Logs,
                Outputs = commandVM.Outputs,
                CreatedBy = ClaimsUtility.GetId(User)
            };

            var result = await Mediator.Send(completeStepCommand);
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
        public async Task<IActionResult> GetAll(int page = 0, int size = 100, string status = null)
        {
            return Ok(await Mediator.Send(new GetStepsQuery()
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
            return Ok(await Mediator.Send(new GetStepQuery()
            {
                Id = id
            }));
        }
    }
}