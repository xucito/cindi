using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.GlobalValues;
using Cindi.Domain.Exceptions.State;
using Cindi.Domain.ValueObjects;
using Cindi.Persistence.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.GlobalValues.Commands.UpdateGlobalValue
{
    public class UpdateGlobalValueCommandHandler : IRequestHandler<UpdateGlobalValueCommand, CommandResult<GlobalValue>>
    {
        ApplicationDbContext _context;

        public UpdateGlobalValueCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<CommandResult<GlobalValue>> Handle(UpdateGlobalValueCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            GlobalValue existingValue;
            if ((existingValue = await _context.GlobalValues.FirstOrDefaultAsync<GlobalValue>(gv => gv.Name == request.Name)) == null)
            {
                throw new InvalidGlobalValuesException("The global value name " + request.Name + " does not exist.");
            }

            existingValue.Value = request.Value;
            existingValue.Description = request.Description;
            _context.Update(existingValue);
            await _context.SaveChangesAsync();
            stopwatch.Stop();

            return new CommandResult<GlobalValue>()
            {
                ObjectRefId = existingValue.Id.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Update,
                Result = existingValue
            };
        }
    }
}
