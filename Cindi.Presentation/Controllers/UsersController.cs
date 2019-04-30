using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Cindi.Application.Users.Commands.CreateUserCommand;
using Cindi.Application.Users.Queries.GetUserQuery;
using Cindi.Application.Users.Queries.GetUsers;
using Cindi.Domain.Entities.Users;
using Cindi.Domain.Exceptions;
using Cindi.Presentation.Results;
using Cindi.Presentation.Utility;
using Cindi.Presentation.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Cindi.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : BaseController
    {
        public UsersController(ILoggerFactory logger) : base(logger.CreateLogger<UsersController>())
        {

        }

        // GET: api/<controller>
        [HttpGet]
        public async Task<IActionResult> Get(int page = 0, int size = 100)
        {
            var username = ClaimsUtility.GetUsername(User);
            var user = await Mediator.Send(new GetUsersQuery() {
                Page = page,
                Size = size
            });

            return Ok(user);
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpGet("me")]
        public async Task<IActionResult> Get()
        {
            var username = ClaimsUtility.GetUsername(User);
            var user = await Mediator.Send(new GetUserQuery()
            {
                Username = username
            });

            return Ok(new HttpQueryResult<User, UserVM>(user,Mapper.Map<UserVM>(user.Result)));
        }

        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post(CreateUserVM request)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                var command = new CreateUserCommand()
                {
                    Username = request.Username,
                    Password = request.Password,
                    CreatedBy = ClaimsUtility.GetId(User)
                };
                var result = await Mediator.Send(command);
                return Ok(new HttpCommandResult<User>("user", result, null));
            }
            catch (BaseException e)
            {
                Logger.LogError(e.Message);
                stopwatch.Stop();
                return BadRequest(e.ToExceptionResult(stopwatch.ElapsedMilliseconds));
            }
        }

        // PUT api/<controller>/5
       /* [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }*/
    }
}
