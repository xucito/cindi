﻿using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.BotKeys.Commands.CreateBotKeyCommand
{
    public class CreateBotKeyCommand: IRequest<CommandResult<string>>
    {
        public string BotKeyName { get; set; }
        public string PublicEncryptionKey { get; set; }
    }
}
