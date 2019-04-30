using Cindi.Application.Results;
using Cindi.Domain.Entities.Users;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Users.Queries.GetUsers
{
    public class GetUsersQuery: IRequest<QueryResult<User[]>>
    {
        public int Page { get; set; }
        public int Size { get; set; }
    }
}
