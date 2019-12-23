using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cindi.Application.Exceptions;
using Cindi.Application.Options;
using Cindi.Application.Results;
using Cindi.Application.Steps.Commands;
using Cindi.Application.Steps.Commands.AppendStepLog;
using Cindi.Application.Steps.Commands.AssignStep;
using Cindi.Application.Steps.Commands.CancelStep;
using Cindi.Application.Steps.Commands.CompleteStep;
using Cindi.Application.Steps.Commands.CreateStep;
using Cindi.Application.Steps.Commands.SuspendStep;
using Cindi.Application.Steps.Commands.UnassignStep;
using Cindi.Application.Steps.Queries;
using Cindi.Application.Steps.Queries.GetStep;
using Cindi.Application.Steps.Queries.GetSteps;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Utilities;
using Cindi.Presentation.Results;
using Cindi.Presentation.Utility;
using Cindi.Presentation.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cindi.Presentation.Controllers
{
    [Authorize]
    public class StepsController : BaseController
    {
        private CindiClusterOptions _option;
        public StepsController(ILoggerFactory logger,
            IOptionsMonitor<CindiClusterOptions> options) : base(logger.CreateLogger<StepsController>())
        {
            _option = options.CurrentValue;
            options.OnChange((change) =>
            {
                _option = change;
            });
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
                    Id = new Guid(result.ObjectRefId),
                    Exclude = (s) => s.Journal
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
            try
            {
                command.BotId = new Guid(ClaimsUtility.GetId(User));
                var result = await Mediator.Send(command);
                if (result.ObjectRefId != "")
                {
                    return Ok(new HttpCommandResult<Step>("step", result, result.Result));
                }
                else
                {
                    return Ok(new HttpCommandResult<Step>("", result, null));
                }
            }
            catch(BotKeyAssignmentException e)
            {
                return BadRequest(command.BotId + " has been disabled for assignment.");
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
            //TODO check that the bot who is updating is the same as the one who was assigned

            if (commandVM.Status == StepStatuses.Suspended)
            {
                var result = await Mediator.Send(new SuspendStepCommand()
                {
                    StepId = id,
                    CreatedBy = ClaimsUtility.GetId(User),
                    SuspendedUntil = DateTime.Now.AddMilliseconds(_option.DefaultSuspensionTimeMs)
                });

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
            else
            {
                var completeStepCommand = new CompleteStepCommand()
                {
                    Id = id,
                    Status = commandVM.Status.ToLower(),
                    StatusCode = commandVM.StatusCode,
                    Log = commandVM.Logs,
                    Outputs = commandVM.Outputs,
                    CreatedBy = ClaimsUtility.GetId(User),
                    BotId = new Guid(ClaimsUtility.GetId(User))
                };
                try
                {
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
                catch (DuplicateStepUpdateException exception)
                {
                    return Ok();
                }
            }
        }

        [HttpPut]
        [Route("{id}/status")]
        public async Task<IActionResult> UpdateStepStatus(Guid id, PutStepStatus putModel)
        {
            IRequest<CommandResult> request;

            switch (putModel.Status)
            {
                case StepStatuses.Suspended:
                    request = new SuspendStepCommand()
                    {
                        StepId = id,
                        CreatedBy = ClaimsUtility.GetId(User),
                        SuspendedUntil = putModel.SuspendedUntil
                    };
                    break;
                case StepStatuses.Cancelled:
                    request = new CancelStepCommand()
                    {
                        StepId = id,
                        CreatedBy = ClaimsUtility.GetId(User)
                    };
                    break;
                case StepStatuses.Unassigned:
                    request = new UnassignStepCommand()
                    {
                        StepId = id,
                        CreatedBy = ClaimsUtility.GetId(User)
                    };
                    break;
                default:
                    return BadRequest(new ExceptionResult()
                    {
                        ExceptionName = "InvalidStepStatusException",
                        Message = "Step status given was not valid. Steps can only be suspended, unassigned or cancelled by users"
                    });
            }

            var result = await Mediator.Send(request);
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
        public async Task<IActionResult> GetAll(int page = 0, int size = 20, string status = null)
        {
            return Ok(await Mediator.Send(new GetStepsQuery()
            {
                Page = page,
                Size = size,
                Status = status,
                Exclusions = new List<Expression<Func<Step, object>>>{
                    (s) => s.Journal
                }
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