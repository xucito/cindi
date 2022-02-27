using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.Users;
using Cindi.Domain.Utilities;
using Nest;
using MediatR;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cindi.Application.Utilities;

namespace Cindi.Application.Users.Commands.AuthenticateUserCommand
{
    public class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, CommandResult>
    {
        ElasticClient _context;

        public AuthenticateUserCommandHandler(ElasticClient context,
                ILogger<AuthenticateUserCommandHandler> logger
            )
        {
            _context = context;

        }
        public async Task<CommandResult> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var user = await _context.FirstOrDefaultAsync<User>(st => st.Query(q => q.Term(f => f.Username, request.Username.ToLower())));

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
