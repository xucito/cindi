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

namespace Cindi.Application.Steps.Commands.UnassignStep
{
    public class UnassignStepCommandHandler : IRequestHandler<UnassignStepCommand, CommandResult>
    {
       
        public ILogger<UnassignStepCommandHandler> Logger;
        private CindiClusterOptions _option;
        private readonly ElasticClient _context;

        public UnassignStepCommandHandler(
            ILogger<UnassignStepCommandHandler> logger,
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


        public async Task<CommandResult> Handle(UnassignStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            Step step;
            if ((step = await _context.FirstOrDefaultAsync<Step>(st => st.Query(q => (q.Term(f => f.Status, StepStatuses.Suspended) || 
            q.Term(f => f.Status, StepStatuses.Assigned)) && 
            q.Term(f => f.Id, request.StepId)))) == null)
            {
                Logger.LogWarning("Step " + request.StepId + " has a status that cannot be unassigned.");
                return new CommandResult()
                {
                    ElapsedMs = stopwatch.ElapsedMilliseconds,
                    ObjectRefId = request.StepId.ToString(),
                    Type = CommandResultTypes.None
                };
            }

            step = await _context.LockObject(step);

            if (step != null)
            {
                if (step.Status != StepStatuses.Suspended && step.Status != StepStatuses.Assigned)
                {
                    Logger.LogWarning("Step " + request.StepId + " has status " + step.Status + ". Only suspended steps can be unassigned.");
                    return new CommandResult()
                    {
                        ElapsedMs = stopwatch.ElapsedMilliseconds,
                        ObjectRefId = step.Id.ToString(),
                        Type = CommandResultTypes.None
                    };
                }

                step.Status = StepStatuses.Unassigned;

                step.AssignedTo = null;

                if ((await _context.IndexDocumentAsync(step)).IsValid)
                {
                    return new CommandResult()
                    {
                        ElapsedMs = stopwatch.ElapsedMilliseconds,
                        ObjectRefId = step.Id.ToString(),
                        Type = CommandResultTypes.Update
                    };
                }
                else
                {
                    throw new FailedClusterOperationException("Failed to apply cluster operation with for step " + step.Id);
                }
            }
            else
            {
                throw new FailedClusterOperationException("Step " + request.StepId + " failed to have a lock applied.");
            }
        }
    }
}
