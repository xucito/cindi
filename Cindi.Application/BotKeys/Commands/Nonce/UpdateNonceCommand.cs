using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.BotKeys.Commands.Nonce
{
    public class UpdateNonceCommand: IRequest<CommandResult>
    {
        public double Nonce { get; set; }
        public Guid Id { get; set; }
    }
}
