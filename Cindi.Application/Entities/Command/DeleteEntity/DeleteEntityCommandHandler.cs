using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities;
using Cindi.Domain.Exceptions.State;
using Cindi.Persistence.Data;
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
        ApplicationDbContext _context;
        public DeleteEntityCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(DeleteEntityCommand<T> request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _context.Remove(request.Entity);

            await _context.SaveChangesAsync();

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
