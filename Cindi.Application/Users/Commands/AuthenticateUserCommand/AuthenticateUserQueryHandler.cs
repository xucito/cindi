using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.Users;
using Cindi.Domain.Utilities;
using MediatR;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Caching.Memory;
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
        IEntitiesRepository _entitiesRepository;
        IMemoryCache _memoryCache;

        public AuthenticateUserCommandHandler(IEntitiesRepository entitiesRepository,
                ILogger<AuthenticateUserCommandHandler> logger,
                IMemoryCache memoryCache
            )
        {
            _entitiesRepository = entitiesRepository;
            _memoryCache = memoryCache;
        }
        public async Task<CommandResult> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            User user;
            if (!_memoryCache.TryGetValue(request.Username.ToLower(), out user))
            {
                user = await _entitiesRepository.GetFirstOrDefaultAsync<User>(u => u.Username == request.Username.ToLower());
                _memoryCache.Set(request.Username.ToLower(), user,
                    new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetSlidingExpiration(TimeSpan.FromSeconds(10)));
            }

            if (user != null)
            {
                if (!user.IsDisabled)
                {
                    string precomputedHash;

                    if (!_memoryCache.TryGetValue(request.Password + Convert.ToBase64String(user.Salt), out precomputedHash))
                    {
                        precomputedHash = SecurityUtility.OneWayHash(request.Password, user.Salt);
                        _memoryCache.Set(request.Password + Convert.ToBase64String(user.Salt), precomputedHash,
                            new MemoryCacheEntryOptions()
                            // Keep in cache for this time, reset time if accessed.
                            .SetSlidingExpiration(TimeSpan.FromSeconds(60)));
                    }

                    if (precomputedHash == user.HashedPassword)
                    {
                        return new CommandResult()
                        {
                            ObjectRefId = user.Id.ToString(),
                            ElapsedMs = stopwatch.ElapsedMilliseconds,
                            Type = CommandResultTypes.None,
                            Result = user
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
