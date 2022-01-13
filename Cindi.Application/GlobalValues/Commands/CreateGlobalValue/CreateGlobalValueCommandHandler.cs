using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Global;
using Cindi.Domain.Exceptions.GlobalValues;
using Cindi.Domain.Utilities;
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

namespace Cindi.Application.GlobalValues.Commands.CreateGlobalValue
{
    public class CreateGlobalValueCommandHandler : IRequestHandler<CreateGlobalValueCommand, CommandResult<GlobalValue>>
    {
        ApplicationDbContext _context;

        public CreateGlobalValueCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<CommandResult<GlobalValue>> Handle(CreateGlobalValueCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (!InputDataTypes.IsValidDataType(request.Type))
            {
                throw new InvalidInputTypeException("Input " + request.Type + " is not valid.");
            }

            if (await _context.GlobalValues.FirstOrDefaultAsync<GlobalValue>(
                gv => gv.Name == request.Name) != null)
            {
                throw new InvalidGlobalValuesException("The global value name " + request.Name + " is already in-use.");
            }

            var createdGV = new GlobalValue()
            {
                Name = request.Name,
                Type = request.Type,
                Description = request.Description,
                Value = request.Type == InputDataTypes.Secret ? SecurityUtility.SymmetricallyEncrypt((string)request.Value, ClusterStateService.GetEncryptionKey()) : request.Value,
                Status = GlobalValueStatuses.Enabled,
                CreatedBy = request.CreatedBy
            };

            _context.Add(createdGV);
            await _context.SaveChangesAsync();

            stopwatch.Stop();

            return new CommandResult<GlobalValue>()
            {
                ObjectRefId = createdGV.Id.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Create,
                Result = createdGV
            };
        }
    }
}
