using Cindi.Application.Results;
using Cindi.Domain.Entities.Users;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Users.Queries.GetUserQuery
{
    public class GetUserQuery: IRequest<QueryResult<User>>
    {
        public string Username { get; set; }
    }
}
