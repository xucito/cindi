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

namespace Cindi.Application.Users.Queries.GetUsers
{
    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, QueryResult<User[]>>
    {
        IUsersRepository _usersRepository;

        public GetUsersQueryHandler(IUsersRepository usersRepository, ILogger<GetUsersQueryHandler> logger)
        {
            _usersRepository = usersRepository;
        }
        public async System.Threading.Tasks.Task<QueryResult<User[]>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var users = await _usersRepository.GetUsersAsync(request.Size, request.Page);
            return new QueryResult<User[]>()
            {
                Result = users.ToArray(),
                Count = users.Count,
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
