using Cindi.Application.Results;
using Cindi.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Cindi.Application.Entities.Command.DeleteEntity
{
    public class AddEntityCommand<T> : IRequest<CommandResult> where T : BaseEntity
    {
        public BaseEntity Entity { get; set; }
    }
}
