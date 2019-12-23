using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.BotKeys.Commands.DeleteBotKeyCommand
{
    public class DeleteBotKeyCommand : IRequest<CommandResult>
    {
        public Guid Id { get; set; }
    }
}
