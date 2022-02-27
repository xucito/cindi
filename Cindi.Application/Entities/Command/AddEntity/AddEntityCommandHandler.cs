using Cindi.Application.Interfaces;
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
    public class AddEntityCommandHandler<T> : IRequestHandler<AddEntityCommand<T>, CommandResult>
        where T : BaseEntity
    {
        ElasticClient _context;
        public AddEntityCommandHandler(ElasticClient context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(AddEntityCommand<T> request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await _context.IndexDocumentAsync(request.Entity);

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
