using Cindi.Application.Workflows.Commands;
using Cindi.Application.WorkflowTemplates.Commands.CreateWorkflowTemplate;
using Cindi.Application.WorkflowTemplates.Queries.GetWorkFlowTemplate;
using Cindi.Application.WorkflowTemplates.Queries.GetWorkflowTemplates;
using Cindi.Domain.Entities.WorkflowsTemplates;
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
    public class WorkflowTemplatesController: BaseController
    {
        public WorkflowTemplatesController(ILoggerFactory logger) : base(logger.CreateLogger<WorkflowTemplatesController>())
        {
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]CreateWorkflowTemplateCommand command)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                command.CreatedBy = ClaimsUtility.GetId(User);
                var result = await Mediator.Send(command);
                return Ok(new HttpCommandResult<WorkflowTemplate>("/api/workflowTemplate/" + command.Name + "/" + command.Version, result, null));
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
            return Ok(await Mediator.Send(new GetWorkflowTemplatesQuery()
            {
                Page = 0,
                Size = 100
            }));
        }

        [HttpGet]
        [Route("{name}/{version}")]
        public async Task<IActionResult> Get(string name, string version)
        {
            return Ok(await Mediator.Send(new GetWorkflowTemplateQuery()
            {
                Id = name + ":" + version
            }));
        }
    }
}
