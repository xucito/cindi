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
using Cindi.Domain.Entities.States;
using Nest;

namespace Cindi.Application.Users.Commands.CreateUserCommand
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CommandResult>
    {
        
        ElasticClient _context;

        public CreateUserCommandHandler(
            
            ILogger<CreateUserCommandHandler> logger,
            IServiceProvider prov,
            ElasticClient context
    )
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var salt = SecurityUtility.GenerateSalt(128);
            Guid id = Guid.NewGuid();
            var createdUser =  await _context.IndexDocumentAsync(new Domain.Entities.Users.User()
            {
                Id = Guid.NewGuid(),
                Username = request.Username.ToLower(),
                HashedPassword = SecurityUtility.OneWayHash(request.Password, salt),
                Email = request.Username.ToLower(),
                Salt = salt,
                CreatedBy = request.CreatedBy
            });

            

            return new CommandResult()
            {
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                ObjectRefId = id.ToString(),
                Type = CommandResultTypes.Create
            };
        }
    }
}
