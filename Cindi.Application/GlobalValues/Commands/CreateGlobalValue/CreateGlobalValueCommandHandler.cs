using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Global;
using Cindi.Domain.Exceptions.GlobalValues;
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

            var createdGV = await _globalValuesRepository.InsertGlobalValue(new GlobalValue()
            {
                Name = request.Name,
                Status = GlobalValueStatuses.Enabled,
                Type = request.Type,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = request.CreatedBy
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
