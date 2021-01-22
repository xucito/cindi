
using Cindi.Application.Interfaces;
using Cindi.Application.Options;
using Cindi.Application.Results;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.State;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Steps.Commands.CancelStep
{
    public class CancelStepCommandHandler : IRequestHandler<CancelStepCommand, CommandResult>
    {
        public ILogger<CancelStepCommandHandler> Logger;
        private CindiClusterOptions _option;
        private readonly IEntitiesRepository _entitiesRepository;
        private readonly IStateMachine _stateMachine;

        public CancelStepCommandHandler(
            ILogger<CancelStepCommandHandler> logger,
            IOptionsMonitor<CindiClusterOptions> options,
            IEntitiesRepository entitiesRepository,
            IStateMachine stateMachine)
        {
            _entitiesRepository = entitiesRepository;
            Logger = logger;
            _stateMachine = stateMachine;
            options.OnChange((change) =>
            {
                _option = change;
            });
        }


        public async Task<CommandResult> Handle(CancelStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var stepLock = _stateMachine.LockEntity<Step>(request.StepId);

            // Applied the lock successfully
            if (stepLock)
            {
                var step = await _entitiesRepository.GetByIdAsync<Step>(request.StepId);
                // You can only cancel steps that are unassigned, suspended or assigned
                if (step.Status == StepStatuses.Unassigned ||
                    step.Status == StepStatuses.Suspended ||
                    step.Status == StepStatuses.Assigned)
                {
                    step.Status = StepStatuses.Cancelled;

                    await _entitiesRepository.Update(step);
                    return new CommandResult()
                    {
                        ElapsedMs = stopwatch.ElapsedMilliseconds,
                        ObjectRefId = step.Id.ToString(),
                        Type = CommandResultTypes.Update
                    };
                }
                else
                {
                    throw new InvalidStepStatusException("Step " + request.StepId + " could not be cancelled as it has status " + step.Status);
                }
            }
            else
            {
                throw new FailedClusterOperationException("Step " + request.StepId + " failed to have a lock applied.");
            }
        }
    }
}
