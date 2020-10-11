using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterOperation;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Global;
using Cindi.Domain.Exceptions.GlobalValues;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.RPCs;
using ConsensusCore.Domain.RPCs.Raft;
using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Domain.SystemCommands;
using ConsensusCore.Node;
using ConsensusCore.Node.Communication.Controllers;
using ConsensusCore.Node.Communication.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.GlobalValues.Commands.CreateGlobalValue
{
    public class CreateGlobalValueCommandHandler : IRequestHandler<CreateGlobalValueCommand, CommandResult<GlobalValue>>
    {
        private readonly IClusterService _clusterService;

        public CreateGlobalValueCommandHandler(
            IClusterService clusterService)
        {
            _clusterService = clusterService;
        }
        public async Task<CommandResult<GlobalValue>> Handle(CreateGlobalValueCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (!InputDataTypes.IsValidDataType(request.Type))
            {
                throw new InvalidInputTypeException("Input " + request.Type + " is not valid.");
            }

            Guid lockId = Guid.NewGuid();
            var command = await _clusterService.Handle(new ExecuteCommands()
            {
                Commands = new List<BaseCommand>{
                                    new SetLock()
                                    {
                                        Name = "action:create:" + request.Name,
                                        LockId = lockId,
                                        TimeoutMs = 60000,
                                        CreatedOn = DateTime.Now
                                    }
                                },
                WaitForCommits = true
            });

            if (command.IsSuccessful)
            {
                var existingGV = await _clusterService.GetFirstOrDefaultAsync<GlobalValue>(gv => gv.Name == request.Name);
                if(existingGV != null)
                {
                    throw new DuplicateGlobalValueException("Global value with name " + request.Name + " already exists.");
                }

                var createdGV = new GlobalValue()
                {
                    Name = request.Name,
                    Type = request.Type,
                    Description = request.Description,
                    Value = request.Type == InputDataTypes.Secret ? SecurityUtility.SymmetricallyEncrypt((string)request.Value, ClusterStateService.GetEncryptionKey()) : request.Value,
                    Status = GlobalValueStatuses.Enabled,
                    Id = Guid.NewGuid()
                };

                var result = _clusterService.Handle(new AddShardWriteOperation()
                {
                    Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Create,
                    WaitForSafeWrite = true,
                    Data = createdGV,
                    RemoveLock = true,
                    LockId = lockId
                });

                stopwatch.Stop();

                return new CommandResult<GlobalValue>()
                {
                    ObjectRefId = createdGV.Id.ToString(),
                    ElapsedMs = stopwatch.ElapsedMilliseconds,
                    Type = CommandResultTypes.Create,
                    Result = createdGV
                };
            }
            else
            {
                throw new WriteConcurrencyException("Two global values being written at the same time.");
            }
        }
    }
}
