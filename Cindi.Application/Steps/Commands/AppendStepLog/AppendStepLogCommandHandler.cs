using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Steps.Commands.AppendStepLog
{
    public class AppendStepLogCommandHandler : IRequestHandler<AppendStepLogCommand, CommandResult>
    {
        public ILogger<AppendStepLogCommandHandler> Logger;
        private readonly IEntitiesRepository _entitiesRepository;
        public AppendStepLogCommandHandler(
            ILogger<AppendStepLogCommandHandler> logger,
            IEntitiesRepository entitiesRepository
            )
        {
            Logger = logger;
            _entitiesRepository = entitiesRepository;
        }

        public async Task<CommandResult> Handle(AppendStepLogCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var step = await _entitiesRepository.GetFirstOrDefaultAsync<Step>(e => e.Id == request.StepId);

            if (StepStatuses.IsCompleteStatus(step.Status))
            {
                throw new InvalidStepStatusException("Cannot append log to step, step status is complete with " + step.Status);
            }

            step.Logs.Add(new StepLog()
            {
                CreatedOn = DateTime.UtcNow,
                Message = request.Log
            });

            await _entitiesRepository.Update(step);

            return new CommandResult()
            {
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                ObjectRefId = step.Id.ToString(),
                Type = CommandResultTypes.Update
            };
        }
    }
}
