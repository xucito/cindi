using Cindi.Application.Entities.Queries;
using Cindi.Application.Entities.Queries.GetEntity;
using Cindi.Application.Workflows.Commands;
using Cindi.Application.Workflows.Commands.CreateWorkflow;
using Cindi.Application.Workflows.Commands.ScanWorkflow;
using Cindi.Domain.Entities.Steps;
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
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Cindi.Presentation.Controllers
{
    public class WorkflowsController : BaseController
    {
        public WorkflowsController(ILoggerFactory logger) : base(logger.CreateLogger<WorkflowsController>())
        {
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateWorkflowCommand command)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (command.Inputs == null)
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
        public async Task<IActionResult> GetAll(int page = 0, int size = 20, string status = null, string sort = "createdOn:1")
        {
            return Ok(await Mediator.Send(new GetEntitiesQuery<Workflow>()
            {
                Expression = e => e.Query(q => q.Term(f => f.Status, status == null ? null : status)).Size(size).Skip(size * page)
            }));
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await Mediator.Send(new GetEntityQuery<Workflow>()
            {
                Expression = w => w.Query(q => q.Term(f => f.Id, id))
            }));
        }

        [HttpGet]
        [Route("{id}/steps")]
        public async Task<IActionResult> GetWorkflowSteps(Guid id)
        {
            return Ok(await Mediator.Send(new GetEntitiesQuery<Step>()
            {
                Expression = (s) => s.Query(q => q.Term(f => f.WorkflowId, id)).Size(1000)
            }));
        }

        [HttpGet]
        [Route("{id}/scan")]
        public async Task<IActionResult> ScanWorkflow(Guid id)
        {
            return Ok(await Mediator.Send(new ScanWorkflowCommand()
            {
                CreatedBy = ClaimsUtility.GetId(User),
                WorkflowId = id
        }));
        }

    }
}
