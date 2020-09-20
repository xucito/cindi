
using Cindi.Application.Interfaces;
using Cindi.Application.Options;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterOperation;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.State;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.ValueObjects;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.RPCs;
using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Node;
using ConsensusCore.Node.Communication.Controllers;
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
        private readonly ClusterService _clusterService;

        public CancelStepCommandHandler(
            ILogger<CancelStepCommandHandler> logger,
            IOptionsMonitor<CindiClusterOptions> options,
            ClusterService clusterService)
        {
            _clusterService = clusterService;
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

            var stepLockResult = await _clusterService.Handle(new RequestDataShard()
            {
                Type = "Step",
                ObjectId = request.StepId,
                CreateLock = true,
                LockTimeoutMs = 10000
            });

            // Applied the lock successfully
            if (stepLockResult.IsSuccessful && stepLockResult.AppliedLocked)
            {
                var step = (Step)stepLockResult.Data;
                // You can only cancel steps that are unassigned, suspended or assigned
                if (step.Status == StepStatuses.Unassigned ||
                    step.Status == StepStatuses.Suspended ||
                    step.Status == StepStatuses.Assigned)
                {
                    step.Status = StepStatuses.Cancelled;

                    var result = await _clusterService.AddWriteOperation(new EntityWriteOperation<Step>()
                    {
                        Data = step,
                        Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Update,
                        User = request.CreatedBy,
                        RemoveLock = true
                    });

                    if (result.IsSuccessful)
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
