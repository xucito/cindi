using Cindi.Application.Interfaces;
using Cindi.Application.Options;
using Cindi.Application.Results;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.State;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.ValueObjects;
using Cindi.Persistence.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Steps.Commands.UnassignStep
{
    public class UnassignStepCommandHandler : IRequestHandler<UnassignStepCommand, CommandResult>
    {
       
        public ILogger<UnassignStepCommandHandler> Logger;
        private CindiClusterOptions _option;
        private readonly ApplicationDbContext _context;

        public UnassignStepCommandHandler(
            ILogger<UnassignStepCommandHandler> logger,
            IOptionsMonitor<CindiClusterOptions> options,
             ApplicationDbContext context)
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
            if ((step = await _context.Steps.FirstOrDefaultAsync(e => (e.Status == StepStatuses.Suspended || e.Status == StepStatuses.Assigned) && e.Id == request.StepId)) == null)
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

                _context.Update(step);


                if (await _context.SaveChangesAsync() > 0)
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
