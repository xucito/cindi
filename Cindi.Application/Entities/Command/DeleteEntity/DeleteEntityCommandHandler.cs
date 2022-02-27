﻿using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities;
using Cindi.Domain.Exceptions.State;
using Nest;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Entities.Command.DeleteEntity
{
    public class DeleteEntityCommandHandler<T> : IRequestHandler<DeleteEntityCommand<T>, CommandResult>
        where T : BaseEntity
    {
        ElasticClient _context;
        public DeleteEntityCommandHandler(ElasticClient context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(DeleteEntityCommand<T> request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _context.Delete<T>(request.Entity.Id);

            return new CommandResult()
            {
                ObjectRefId = request.Entity.Id.ToString(),
                Type = CommandResultTypes.Delete,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                IsSuccessful = true
            };

        }
    }
}
