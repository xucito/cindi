using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Cindi.Application.BotKeys.Commands.CreateBotKeyCommand
{
    public class CreateBotKeyCommand: IRequest<CommandResult<string>>
    {
        [Required]
        public string BotKeyName { get; set; }

        [Required]
        public string PublicEncryptionKey { get; set; }
    }
}
