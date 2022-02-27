using Cindi.Application.Entities.Queries;
using Cindi.Application.Entities.Queries.GetEntity;
using Cindi.Application.Workflows.Commands;
using Cindi.Application.WorkflowTemplates.Commands.CreateWorkflowTemplate;
using Cindi.Domain.Entities.WorkflowsTemplates;
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
using System.Linq.Expressions;
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

            if(command.InputDefinitions == null)
            {
                command.InputDefinitions = new Dictionary<string, Domain.ValueObjects.DynamicDataDescription>();
            }

            if(command.LogicBlocks == null)
            {
                command.InputDefinitions = new Dictionary<string, Domain.ValueObjects.DynamicDataDescription>();
            }
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
        public async Task<IActionResult> GetAll(int page = 0, int size = 100, string sort = "CreatedOn:1")
        {
            return Ok(await Mediator.Send(new GetEntitiesQuery<WorkflowTemplate>()
            {
                Expression = (s => s.Size(size).Skip(page * size))
            }));
        }

        [HttpGet]
        [Route("{name}/{version}")]
        public async Task<IActionResult> Get(string name, string version)
        {
            return Ok(await Mediator.Send(new GetEntityQuery<WorkflowTemplate>()
            {
                Expression = s => s.Query(st => st.Term(a => a.ReferenceId, name + ":" + version))
            }));
        }
    }
}
