using Cindi.Application.Results;
using Cindi.Domain.Entities.BotKeys;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.BotKeys.Commands.UpdateBotKeyCommand
{
    public class UpdateBotKeyCommand : IRequest<CommandResult<BotKey>>
    {
        public Guid Id { get; set; }
        /// <summary>
        /// Set to null to not update the value
        /// </summary>
        public bool? IsDisabled { get; set; }
        /// <summary>
        /// Set to null to not update
        /// </summary>
        public string BotName { get; set; }
    }
}
