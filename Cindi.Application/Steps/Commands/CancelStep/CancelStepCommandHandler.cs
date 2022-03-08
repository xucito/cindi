using Cindi.Application.Interfaces;
using Cindi.Application.Options;
using Cindi.Application.Results;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.State;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.ValueObjects;
using Nest;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cindi.Application.Utilities;

namespace Cindi.Application.Steps.Commands.CancelStep
{
    public class CancelStepCommandHandler : IRequestHandler<CancelStepCommand, CommandResult>
    {
        public ILogger<CancelStepCommandHandler> Logger;
        private CindiClusterOptions _option;
        private readonly ElasticClient _context;

        public CancelStepCommandHandler(
            ILogger<CancelStepCommandHandler> logger,
            IOptionsMonitor<CindiClusterOptions> options,
             ElasticClient context)
        {
            _context = context;
            Logger = logger;
            options.OnChange((change) =>
            {
                _option = change;
            });
        }


        public async Task<CommandResult> Handle(CancelStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Step step = await _context.LockAndGetObject<Step>(request.StepId);
            // Applied the lock successfully
            if (step != null)
            {
                // You can only cancel steps that are unassigned, suspended or assigned
                if (step.Status == StepStatuses.Unassigned ||
                    step.Status == StepStatuses.Suspended ||
                    step.Status == StepStatuses.Assigned)
                {
                    step.Status = StepStatuses.Cancelled;
                    await _context.IndexDocumentAsync(step);
                    await _context.Unlock<Step>(step.Id);

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
