
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterOperation;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.GlobalValues;
using Cindi.Domain.Exceptions.State;
using Cindi.Domain.ValueObjects;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.RPCs;
using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Node;
using ConsensusCore.Node.Communication.Controllers;
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
        IClusterService _clusterService;

        public UpdateGlobalValueCommandHandler(IClusterService clusterService)
        {
            _clusterService = clusterService;
        }
        public async Task<CommandResult<GlobalValue>> Handle(UpdateGlobalValueCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            GlobalValue existingValue;

            if ((existingValue = await _clusterService.GetFirstOrDefaultAsync<GlobalValue>(gv => gv.Name == request.Name)) == null)
            {
                throw new InvalidGlobalValuesException("The global value name " + request.Name + " does not exist.");
            }

            var globalValueLock = await _clusterService.Handle(new RequestDataShard()
            {
                Type = existingValue.ShardType,
                ObjectId = existingValue.Id,
                CreateLock = true
            });

            if (globalValueLock.IsSuccessful && globalValueLock.AppliedLocked)
            {
                existingValue = (GlobalValue)globalValueLock.Data;
                existingValue.Value = request.Value;
                existingValue.Description = request.Description;

                await _clusterService.AddWriteOperation(new EntityWriteOperation<GlobalValue>()
                {
                    Data = existingValue,
                    Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Update,
                    User = request.CreatedBy,
                    RemoveLock = true
                });

                stopwatch.Stop();

                return new CommandResult<GlobalValue>()
                {
                    ObjectRefId = existingValue.Id.ToString(),
                    ElapsedMs = stopwatch.ElapsedMilliseconds,
                    Type = CommandResultTypes.Update,
                    Result = existingValue
                };
            }
            else
            {
                throw new FailedClusterOperationException("Global Value " + request.Name + " failed to have a lock applied.");
            }
        }
    }
}
