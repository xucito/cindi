using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Global;
using Cindi.Domain.Exceptions.GlobalValues;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.RPCs;
using ConsensusCore.Node;
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
        IGlobalValuesRepository _globalValuesRepository { get; set; }
        IConsensusCoreNode<CindiClusterState, IBaseRepository> _node;

        public CreateGlobalValueCommandHandler(IGlobalValuesRepository globalValuesRepository, IConsensusCoreNode<CindiClusterState, IBaseRepository> node)
        {
            _globalValuesRepository = globalValuesRepository;
            _node = node;
        }
        public async Task<CommandResult<GlobalValue>> Handle(CreateGlobalValueCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (!InputDataTypes.IsValidDataType(request.Type))
            {
                throw new InvalidInputTypeException("Input " + request.Type + " is not valid.");
            }

            if (await _globalValuesRepository.GetGlobalValueAsync(request.Name) != null)
            {
                throw new InvalidGlobalValuesException("The global value name " + request.Name + " is already in-use.");
            }

            var createdGV = new GlobalValue(
                request.Name,
                request.Type,
                request.Description,
                request.Type == InputDataTypes.Secret ? SecurityUtility.SymmetricallyEncrypt((string)request.Value, ClusterStateService.GetEncryptionKey()) : request.Value,
                GlobalValueStatuses.Enabled,
                Guid.NewGuid(),
                request.CreatedBy,
                DateTime.UtcNow
                );

            var result = _node.Send(new WriteData()
            {
                Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Create,
                WaitForSafeWrite = true,
                Data = createdGV
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
    }
}
