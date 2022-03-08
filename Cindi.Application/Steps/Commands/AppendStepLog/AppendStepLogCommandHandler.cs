using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.ValueObjects;
using Nest;
using MediatR;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cindi.Application.Utilities;

namespace Cindi.Application.Steps.Commands.AppendStepLog
{
    public class AppendStepLogCommandHandler : IRequestHandler<AppendStepLogCommand, CommandResult>
    {
        public ILogger<AppendStepLogCommandHandler> Logger;
        private readonly ElasticClient _context;

        public AppendStepLogCommandHandler(
            ILogger<AppendStepLogCommandHandler> logger,
            ElasticClient context
            )
        {
            Logger = logger;
            _context = context;
        }

        public async Task<CommandResult> Handle(AppendStepLogCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var step = await _context.FirstOrDefaultAsync<Step>(request.StepId);

            if (StepStatuses.IsCompleteStatus(step.Status))
            {
                throw new InvalidStepStatusException("Cannot append log to step, step status is complete with " + step.Status);
            }

            if(step.Logs == null)
            {
                step.Logs = new List<StepLog>();
            }

            step.Logs.Add(new StepLog()
            {
                CreatedOn = DateTime.UtcNow,
                Message = request.Log
            });


            return new CommandResult()
            {
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                ObjectRefId = step.Id.ToString(),
                Type = CommandResultTypes.Update
            };
        }
    }
}
