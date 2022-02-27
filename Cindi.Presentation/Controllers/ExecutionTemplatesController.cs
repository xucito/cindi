using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cindi.Application.Entities.Queries;
using Cindi.Application.Entities.Queries.GetEntity;
using Cindi.Application.ExecutionTemplates.Commands;
using Cindi.Application.ExecutionTemplates.Commands.CreateExecutionTemplate;
using Cindi.Application.ExecutionTemplates.Commands.ExecuteExecutionTemplate;
using Cindi.Application.Utilities;
using Cindi.Domain.Entities.ExecutionTemplates;
using Cindi.Domain.Exceptions;
using Cindi.Presentation.Results;
using Cindi.Presentation.Utility;
using Cindi.Presentation.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Cindi.Presentation.Controllers
{
    public class ExecutionTemplatesController : BaseController
    {
        public ExecutionTemplatesController(ILoggerFactory logger) : base(logger.CreateLogger<ExecutionTemplatesController>())
        {
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]CreateExecutionTemplateVM command)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            try
            {

                var result = await Mediator.Send(new CreateExecutionTemplateCommand() {
                    CreatedBy = ClaimsUtility.GetId(User),
                    Description = command.Description,
                    ExecutionTemplateType = command.ExecutionTemplateType.ToLower(),
                    Inputs = command.Inputs,
                    Name = command.Name,
                    ReferenceId = command.ReferenceId
                });


                return Ok(new HttpCommandResult<ExecutionTemplate>("/api/execution-template/" + command.Name, result, null));
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
            return Ok(await Mediator.Send(new GetEntitiesQuery<ExecutionTemplate>()
            {
                Expression = e => e.Skip(page * size).Size(size).Sort(sort)
            }));
        }

        [HttpGet]
        [Route("{name}")]
        public async Task<IActionResult> Get(string name)
        {
            return Ok(await Mediator.Send(new GetEntityQuery<ExecutionTemplate>()
            {
                Expression = st => st.Query(q => q.Term(f => f.Name, name))
            }));
        }

        [HttpPost]
        [Route("{name}/execute")]
        public async Task<IActionResult> Execute(string name)
        {
            return Ok(await Mediator.Send(new ExecuteExecutionTemplateCommand()
            {
                Name = name,
                CreatedBy = ClaimsUtility.GetId(User)
            }));
        }
    }
}
