using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Global;
using Cindi.Domain.Exceptions.GlobalValues;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using MediatR;
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
        IGlobalValuesRepository _globalValuesRepository { get; set; }

        public CreateGlobalValueCommandHandler(IGlobalValuesRepository globalValuesRepository)
        {
            _globalValuesRepository = globalValuesRepository;
        }
        public async Task<CommandResult<GlobalValue>> Handle(CreateGlobalValueCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (!InputDataTypes.IsValidDataType(request.Type))
            {
                throw new InvalidInputTypeException("Input " + request.Type + " is not valid.");
            }

            if(await _globalValuesRepository.GetGlobalValueAsync(request.Name) != null)
            {
                throw new InvalidGlobalValuesException("The global value name "+ request.Name +" is already in-use.");
            }

            var createdGV = await _globalValuesRepository.InsertGlobalValue(new GlobalValue(
                request.Name,
                request.Type,
                request.Description,
                request.Type == InputDataTypes.Secret ? SecurityUtility.SymmetricallyEncrypt((string)request.Value, ClusterStateService.GetEncryptionKey()): request.Value,
                GlobalValueStatuses.Enabled,
                Guid.NewGuid(),
                request.CreatedBy,
                DateTime.UtcNow
                )
            {
            });

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
