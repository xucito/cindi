using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Users.Queries.GetUserQuery
{
    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, QueryResult<User>>
    {
        IUsersRepository _usersRepository;

        public GetUserQueryHandler(IUsersRepository usersRepository, ILogger<GetUserQueryHandler> logger)
        {
            _usersRepository = usersRepository;
        }
        public async Task<QueryResult<User>> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var user = await _usersRepository.GetUserAsync(request.Username);
            return new QueryResult<User>() {
                Result = user,
                Count = user == null ? 0 : 1,
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };

        }
    }
}
