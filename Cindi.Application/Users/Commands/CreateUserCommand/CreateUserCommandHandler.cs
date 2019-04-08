using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Diagnostics;
using Cindi.Domain.Utilities;

namespace Cindi.Application.Users.Commands.CreateUserCommand
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CommandResult>
    {
        IUsersRepository _usersRepository;

        public CreateUserCommandHandler(IUsersRepository usersRepository,
    ILogger<CreateUserCommandHandler> logger
    )
        {
            _usersRepository = usersRepository;
        }

        public async Task<CommandResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var salt = SecurityUtility.GenerateSalt(128);

            var createdUser = await _usersRepository.InsertUserAsync(new Domain.Entities.Users.User()
            {
                CreatedOn = DateTime.UtcNow,
                CreatedBy = request.CreatedBy,
                Email = request.Username.ToLower(),
                Username = request.Username.ToLower(),
                Salt = salt,
                HashedPassword = SecurityUtility.OneWayHash(request.Password, salt)
            });

            return new CommandResult()
            {
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                ObjectRefId = createdUser.Username,
                Type = CommandResultTypes.Create
            };
        }
    }
}
