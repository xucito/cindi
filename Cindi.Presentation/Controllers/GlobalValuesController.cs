using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cindi.Application.Entities.Queries;
using Cindi.Application.Entities.Queries.GetEntity;
using Cindi.Application.GlobalValues.Commands.CreateGlobalValue;
using Cindi.Application.GlobalValues.Commands.UpdateGlobalValue;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Presentation.Results;
using Cindi.Presentation.Utility;
using Cindi.Presentation.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Cindi.Presentation.Controllers
{
    public class GlobalValuesController : BaseController
    {
        public GlobalValuesController(ILogger<GlobalValue> logger) : base(logger)
        {
        }

        // GET: api/<controller>
        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 0, int size = 20, string status = null, string sort = "CreatedOn:1")
        {
            var globalValues = await Mediator.Send(new GetEntitiesQuery<GlobalValue>()
            {
                Page = page,
                Size = size,
                Expression = ExpressionBuilder.GenerateExpression(new List<Expression<Func<GlobalValue, bool>>> {
                   status == null ? null : ExpressionBuilder.BuildPredicate<GlobalValue>("Status", OperatorComparer.Equals, status)
                }),
                Sort = sort
            });

            return Ok(new HttpQueryResult<List<GlobalValue>, List<GlobalValueVM>>(globalValues, Mapper.Map<List<GlobalValueVM>>(globalValues.Result)));
        }

        // GET api/<controller>/5
        [HttpGet("{name}")]
        public async Task<IActionResult> Get(string name)
        {
            var globalValue = await Mediator.Send(new GetEntityQuery<GlobalValue>()
            {
                Expression = gv => gv.Name == name
            });

            return Ok(new HttpQueryResult<GlobalValue, GlobalValueVM>(globalValue, Mapper.Map<GlobalValueVM>(globalValue.Result)));
        }

        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]CreateGlobalValueVM globalValue)
        {
            var result = await Mediator.Send(new CreateGlobalValueCommand()
            {
                Description = globalValue.Description,
                Name = globalValue.Name,
                Type = globalValue.Type.ToLower(),
                Value = globalValue.Value,
                CreatedBy = ClaimsUtility.GetId(User)
            });

            return Ok(new HttpCommandResult<GlobalValue, GlobalValueVM>("//api//global-values//" + result.ObjectRefId, result, Mapper.Map<GlobalValueVM>(result.Result)));
        }

        [HttpPut("{name}")]
        public async Task<IActionResult> Put(string name, [FromBody]UpdateGlobalValueVM globalValue)
        {
            var result = await Mediator.Send(new UpdateGlobalValueCommand()
            {
                Description = globalValue.Description,
                Name = name,
                Value = globalValue.Value,
                CreatedBy = ClaimsUtility.GetId(User)
            });

            return Ok(new HttpCommandResult<GlobalValue>("//api//global-values//" + result.ObjectRefId, result, result.Result));
        }
    }
}
