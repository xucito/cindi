using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cindi.Application.GlobalValues.Commands.CreateGlobalValue;
using Cindi.Application.GlobalValues.Queries.GetGlobalValue;
using Cindi.Application.GlobalValues.Queries.GetGlobalValues;
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
        public async Task<IActionResult> Get(int page = 0, int size = 10)
        {
            var globalValues = await Mediator.Send(new GetGlobalValuesQuery()
            {
                Page = page,
                Size = size
            });

            return Ok(new HttpQueryResult<IEnumerable<GlobalValue>,IEnumerable<GlobalValueVM>>(globalValues,Mapper.Map<IEnumerable<GlobalValueVM>>(globalValues.Result)));
        }

        // GET api/<controller>/5
        [HttpGet("{name}")]
        public async Task<IActionResult> Get(string name)
        {
            var globalValue = await Mediator.Send(new GetGlobalValueQuery()
            {
                Name = name
            });

            return Ok(new HttpQueryResult<GlobalValue,GlobalValueVM>(globalValue, Mapper.Map<GlobalValueVM>(globalValue.Result)));
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
    }
}
