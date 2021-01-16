using Cindi.Application.Results;
using Cindi.Domain.Entities;

using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Node.Communication.Controllers;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Entities.Command.CreateTrackedEntity
{
    public class WriteEntityCommandHandler<T> : IRequestHandler<WriteEntityCommand<T>, CommandResult> where T : BaseEntity
    {
        IClusterRequestHandler _node;
        ILogger _logger;
        public WriteEntityCommandHandler(IClusterRequestHandler node,
            ILogger<WriteEntityCommand<T>> logger)
        {
            _node = node;
            _logger = logger;
        }

        public async Task<CommandResult> Handle(WriteEntityCommand<T> request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (request.Data.GetType().IsSubclassOf(typeof(TrackedEntity)))
            {
                if (request.User == "" || request.User == null)
                {
                    _logger.LogError("A tracked record of type " + request.Data.GetType().Name + " was changed without a user identified");
                    throw new Exception("A tracked record of type " + request.Data.GetType().Name + " was changed without a user identified");
                }

                var converted = (TrackedEntity)(ShardData)request.Data;
                if (request.Operation == ConsensusCore.Domain.Enums.ShardOperationOptions.Create)
                {
                    converted.CreatedOn = DateTime.Now;
                    converted.CreatedBy = request.User;
                }
                converted.ModifiedOn = DateTime.Now;
                converted.ModifiedBy = request.User;
            }

            var result = await _node.Handle(new AddShardWriteOperation()
            {
                Data = request.Data,
                WaitForSafeWrite = request.WaitForSafeWrite,
                Operation = request.Operation,
                RemoveLock = request.RemoveLock,
                LockId = request.LockId
            });

            return new CommandResult()
            {
                ObjectRefId = request.Data.Id.ToString(),
                Type = CommandResult.ConvertShardOperationOption(request.Operation),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                IsSuccessful = result.IsSuccessful
            };

        }
    }
}
