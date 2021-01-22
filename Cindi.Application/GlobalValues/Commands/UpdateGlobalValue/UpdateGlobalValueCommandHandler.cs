
using Cindi.Application.Interfaces;
using Cindi.Application.Results;

using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.GlobalValues;
using Cindi.Domain.Exceptions.State;
using Cindi.Domain.ValueObjects;
using MediatR;
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
        private readonly IEntitiesRepository _entitiesRepository;
        private readonly IStateMachine _stateMachine;

        public UpdateGlobalValueCommandHandler(
            IEntitiesRepository entitiesRepository,
            IStateMachine stateMachine)
        {
            _entitiesRepository = entitiesRepository;
            _stateMachine = stateMachine;
        }
        public async Task<CommandResult<GlobalValue>> Handle(UpdateGlobalValueCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            GlobalValue existingValue;

            if ((existingValue = await _entitiesRepository.GetFirstOrDefaultAsync<GlobalValue>(gv => gv.Name == request.Name)) == null)
            {
                throw new InvalidGlobalValuesException("The global value name " + request.Name + " does not exist.");
            }

            existingValue = await _entitiesRepository.GetByIdAsync<GlobalValue>(existingValue.Id);
            existingValue.Value = request.Value;
            existingValue.Description = request.Description;

            await _entitiesRepository.Update(existingValue);

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
