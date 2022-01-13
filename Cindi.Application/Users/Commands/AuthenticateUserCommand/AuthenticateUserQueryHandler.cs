using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.Users;
using Cindi.Domain.Utilities;
using Cindi.Persistence.Data;
using MediatR;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
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
        ApplicationDbContext _context;

        public AuthenticateUserCommandHandler(ApplicationDbContext context,
                ILogger<AuthenticateUserCommandHandler> logger
            )
        {
            _context = context;

        }
        public async Task<CommandResult> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var user = await _context.Users.FirstOrDefaultAsync<User>(u => u.Username == request.Username.ToLower());

            if (user != null)
            {
                if (!user.IsDisabled)
                {
                    if (SecurityUtility.IsMatchingHash(request.Password, user.HashedPassword, user.Salt))
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
