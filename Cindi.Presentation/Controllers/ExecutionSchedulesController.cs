using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cindi.Application.Entities.Command.DeleteEntity;
using Cindi.Application.Entities.Queries;
using Cindi.Application.Entities.Queries.GetEntity;
using Cindi.Application.ExecutionSchedules.Commands.CreateExecutionSchedule;
using Cindi.Application.ExecutionSchedules.Commands.UpdateExecutionSchedule;
using Cindi.Domain.Entities.ExecutionSchedule;
using Cindi.Domain.Exceptions;
using Cindi.Presentation.Results;
using Cindi.Presentation.Utility;
using Cindi.Presentation.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Cindi.Presentation.Controllers
{
    public class ExecutionSchedulesController : BaseController
    {
        public ExecutionSchedulesController(ILoggerFactory logger) : base(logger.CreateLogger<ExecutionSchedulesController>())
        {
        }

        [HttpPost()]
        public async Task<IActionResult> Create([FromBody]CreateExecutionScheduleVM command, bool? runImmediately = false)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {

                var result = await Mediator.Send(new CreateExecutionScheduleCommand()
                {
                    CreatedBy = ClaimsUtility.GetId(User),
                    Description = command.Description,
                    Name = command.Name,
                    RunImmediately =  runImmediately.HasValue ? runImmediately.Value : false,
                    ExecutionTemplateName = command.ExecutionTemplateName,
                    Schedule = command.Schedule
                });


                return Ok(new HttpCommandResult<ExecutionSchedule>("/api/execution-schedules/" + command.Name, result, null));
            }
            catch (BaseException e)
            {
                Logger.LogError(e.Message);
                stopwatch.Stop();
                return BadRequest(e.ToExceptionResult(stopwatch.ElapsedMilliseconds));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 0, int size = 100, string sort = "CreatedOn:1")
        {
            return Ok(await Mediator.Send(new GetEntitiesQuery<ExecutionSchedule>()
            {
                Page = page,
                Size = size,
                Expression = null,
                Sort = sort
            }));
        }

        [HttpGet]
        [Route("{name}")]
        public async Task<IActionResult> Get(string name)
        {
            return Ok(await Mediator.Send(new GetEntityQuery<ExecutionSchedule>()
            {
                Expression = st => st.Name == name
            }));
        }

        [HttpPut]
        [Route("{name}")]
        public async Task<IActionResult> Update(string name, UpdateExecutionScheduleVM update, bool? runImmediately = false)
        {
            return Ok(await Mediator.Send(new UpdateExecutionScheduleCommand()
            {
                Name = name,
                Description = update.Description,
                IsDisabled = update.IsDisabled,
                Schedule = update.Schedule,
                RunImmediately = runImmediately.HasValue ? runImmediately.Value : false
            }));
        }

        [HttpDelete]
        [Route("{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            var result = await Mediator.Send(new GetEntityQuery<ExecutionSchedule>()
            {
                Expression = de => de.Name == name
            });
            return Ok(await Mediator.Send(new DeleteEntityCommand<ExecutionSchedule>()
            {
                Entity = result.Result
            }));
        }
    }
}
