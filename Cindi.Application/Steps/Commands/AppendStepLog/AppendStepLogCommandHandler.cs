
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterOperation;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.ValueObjects;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.RPCs;
using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Node;
using ConsensusCore.Node.Communication.Controllers;
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
        public IClusterService _clusterService;
        public ILogger<AppendStepLogCommandHandler> Logger;

        public AppendStepLogCommandHandler(
            ILogger<AppendStepLogCommandHandler> logger,
            IClusterService clusterService
            )
        {
            Logger = logger;
            _clusterService = clusterService;
        }

        public async Task<CommandResult> Handle(AppendStepLogCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var step = await _clusterService.GetFirstOrDefaultAsync<Step>(e => e.Id == request.StepId);

            if (StepStatuses.IsCompleteStatus(step.Status))
            {
                throw new InvalidStepStatusException("Cannot append log to step, step status is complete with " + step.Status);
            }

            step.Logs.Add(new StepLog()
            {
                CreatedOn = DateTime.UtcNow,
                Message = request.Log
            });

            await _clusterService.AddWriteOperation(new EntityWriteOperation<Step>() {
                Data = step,
                User = request.CreatedBy,
                Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Update
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
