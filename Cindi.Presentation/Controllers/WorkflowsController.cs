﻿using Cindi.Application.Entities.Queries;
using Cindi.Application.Entities.Queries.GetEntity;
using Cindi.Application.Workflows.Commands;
using Cindi.Application.Workflows.Commands.CreateWorkflow;
using Cindi.Application.Workflows.Queries.GetWorkflowSteps;
using Cindi.Domain.Entities.Workflows;
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
    public class WorkflowsController: BaseController
    {
        public WorkflowsController(ILoggerFactory logger) : base(logger.CreateLogger<WorkflowsController>())
        {
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateWorkflowCommand command)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if(command.Inputs == null)
            {
                //Set to an empty dictionary if null
                command.Inputs = new Dictionary<string, object>();
            }
            try
            {
                command.CreatedBy = ClaimsUtility.GetId(User);
                var result = await Mediator.Send(command);
                return Ok(new HttpCommandResult<Workflow>("workflow", result, result.Result));
            }
            catch (BaseException e)
            {
                Logger.LogError(e.Message);
                stopwatch.Stop();
                return BadRequest(e.ToExceptionResult(stopwatch.ElapsedMilliseconds));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 0, int size = 20, string status = null)
        {
            return Ok(await Mediator.Send(new GetEntitiesQuery<Workflow>()
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
            return Ok(await Mediator.Send(new GetEntityQuery<Workflow>()
            {
                Id = id
            }));
        }

        [HttpGet]
        [Route("{id}/steps")]
        public async Task<IActionResult> GetWorkflowSteps(Guid id)
        {
            return Ok(await Mediator.Send(new GetWorkflowStepsQuery()
            {
                WorkflowId = id
            }));
        }

    }
}
