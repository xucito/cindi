using Cindi.Application.Results;
using ConsensusCore.Domain.BaseClasses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Cindi.Application.Entities.Command.DeleteEntity
{
    public class DeleteEntityCommand<T> : IRequest<CommandResult> where T : ShardData
    {
        public Expression<Func<T, bool>> Expression { get; set; }
    }
}
