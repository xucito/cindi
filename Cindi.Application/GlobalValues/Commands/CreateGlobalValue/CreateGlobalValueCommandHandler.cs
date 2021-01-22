using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Global;
using Cindi.Domain.Exceptions.GlobalValues;
using Cindi.Domain.Exceptions.Steps;
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
        private readonly IEntitiesRepository _entitiesRepository;
        private readonly IStateMachine _stateMachine;

        public CreateGlobalValueCommandHandler(
            IEntitiesRepository entitiesRepository,
            IStateMachine stateMachine)
        {
            _entitiesRepository = entitiesRepository;
            _stateMachine = stateMachine;
        }
        public async Task<CommandResult<GlobalValue>> Handle(CreateGlobalValueCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (!InputDataTypes.IsValidDataType(request.Type))
            {
                throw new InvalidInputTypeException("Input " + request.Type + " is not valid.");
            }

            var existingGV = await _entitiesRepository.GetFirstOrDefaultAsync<GlobalValue>(gv => gv.Name == request.Name);
            if (existingGV != null)
            {
                throw new DuplicateGlobalValueException("Global value with name " + request.Name + " already exists.");
            }

            var createdGV = new GlobalValue()
            {
                Name = request.Name,
                Type = request.Type,
                Description = request.Description,
                Value = request.Type == InputDataTypes.Secret ? SecurityUtility.SymmetricallyEncrypt((string)request.Value, _stateMachine.EncryptionKey) : request.Value,
                Status = GlobalValueStatuses.Enabled,
                Id = Guid.NewGuid()
            };

            await _entitiesRepository.Insert(createdGV);

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
