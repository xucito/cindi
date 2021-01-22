using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cindi.Application.BotKeys.Commands.CreateBotKeyCommand;
using Cindi.Application.BotKeys.Commands.DeleteBotKeyCommand;
using Cindi.Application.BotKeys.Commands.UpdateBotKeyCommand;
using Cindi.Application.Entities.Queries;
using Cindi.Application.Entities.Queries.GetEntity;
using Cindi.Application.Interfaces;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Utilities;
using Cindi.Presentation.Results;
using Cindi.Presentation.Utility;
using Cindi.Presentation.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Cindi.Presentation.Controllers
{
    [AllowAnonymous]
    public class BotKeysController : BaseController
    {
        IStateMachine _stateMachine;
        public BotKeysController(ILoggerFactory logger,
            IStateMachine stateMachine) : base(logger.CreateLogger<BotKeysController>())
        {
            _stateMachine = stateMachine;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(CreateBotKeyCommand command)
        {
            if (!_stateMachine.GetSettings.AllowAutoRegistration)
            {
                if (ClaimsUtility.GetId(User) == null)
                {
                    return Unauthorized();
                }
            }

            var keyCreationResult = await Mediator.Send(command);

            var key = await Mediator.Send(new GetEntityQuery<BotKey>()
            {
                Expression = bk => bk.Id == new Guid(keyCreationResult.ObjectRefId)
            });

            return Ok(new HttpCommandResult<NewBotKeyVM>("", keyCreationResult, new NewBotKeyVM()
            {
                // BotName = key.Result.BotName,
                IdKey = keyCreationResult.ObjectRefId
            }));
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 0, int size = 20, string isDisabled = null, string sort = "RegisteredOn:1")
        {
            var keys = await Mediator.Send(new GetEntitiesQuery<BotKey>()
            {
                Page = page,
                Size = size,
                Expression = ExpressionBuilder.GenerateExpression(new List<Expression<Func<BotKey, bool>>> {
                   isDisabled == null ? null : ExpressionBuilder.BuildPredicate<BotKey>("IsDisabled", OperatorComparer.Equals, isDisabled)
                }),
                Sort = sort
            });

            return Ok(new HttpQueryResult<List<BotKey>, List<GetBotKeyVM>>(keys, Mapper.Map<List<GetBotKeyVM>>(keys.Result)));
        }

        [Authorize]
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateBotkeys(Guid id, PutBotKeyVM update)
        {
            var keys = await Mediator.Send(new UpdateBotKeyCommand()
            {
                Id = id,
                BotName = update.BotName,
                IsDisabled = update.IsDisabled
            });

            return Ok(new HttpCommandResult<GetBotKeyVM>("", keys, Mapper.Map<GetBotKeyVM>(keys.Result)));
        }

        [Authorize]
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteBotkey(Guid id)
        {
            var keys = await Mediator.Send(new DeleteBotKeyCommand()
            {
                Id = id
            });

            return Ok(new HttpCommandResult(keys));
        }
    }
}
