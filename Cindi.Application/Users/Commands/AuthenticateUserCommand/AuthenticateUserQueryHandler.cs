using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Utilities;
using MediatR;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Users.Commands.AuthenticateUserCommand
{
    public class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, CommandResult>
    {
        IUsersRepository _usersRepository;

        public AuthenticateUserCommandHandler(IUsersRepository usersRepository,
                ILogger<AuthenticateUserCommandHandler> logger
            )
        {
            _usersRepository = usersRepository;

        }
        public async Task<CommandResult> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var user = await _usersRepository.GetUserAsync(request.Username.ToLower());

            if(user != null)
            {
                if(!user.IsDisabled)
                {
                    if(SecurityUtility.IsMatchingHash(request.Password, user.HashedPassword, user.Salt))
                    {
                        return new CommandResult()
                        {
                            ObjectRefId = user.Username,
                            ElapsedMs = stopwatch.ElapsedMilliseconds,
                            Type = CommandResultTypes.None
                        };
                    }
                }
            }
            return new CommandResult()
            {
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                ObjectRefId = null,
                Type = CommandResultTypes.None
            };
        }
    }
}
