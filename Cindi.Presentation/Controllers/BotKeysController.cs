using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cindi.Application.BotKeys.Commands.CreateBotKeyCommand;
using Cindi.Application.BotKeys.Queries.GetBotKey;
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
        ClusterStateService _clusterState;
        public BotKeysController(ILoggerFactory logger, ClusterStateService clusterState) : base(logger.CreateLogger<BotKeysController>())
        {
            _clusterState = clusterState;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(CreateBotKeyCommand command)
        {
            if (!_clusterState.AutoRegistrationEnabled)
            {
                if (ClaimsUtility.GetId(User) == null)
                {
                    return Unauthorized();
                }
            }

            var keyCreationResult = await Mediator.Send(command);

            var key = await Mediator.Send(new GetBotKeyQuery()
            {
                Id = new Guid(keyCreationResult.ObjectRefId)
            });

            return Ok(new HttpCommandResult<NewBotKeyVM>("", keyCreationResult, new NewBotKeyVM() {
                BotName = key.Result.BotName,
                IdKey = keyCreationResult.ObjectRefId
            }));
        }
    }
}
