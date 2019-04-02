using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Users.Commands.AuthenticateUserCommand
{
    public class AuthenticateUserCommand: IRequest<CommandResult>
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
