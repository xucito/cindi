using Cindi.Domain.Entities;
using Cindi.Persistence.DTOs;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Node.Communication.Controllers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Persistence
{
    public class ClusterRepository
    {
        public IClusterRequestHandler _node;
        public ILogger _logger;

        public ClusterRepository(IClusterRequestHandler node)
        {
            _node = node;
        }

        public async Task<AddShardWriteOperationResponse> AddWriteOperation<T>(WriteEntityOperation<T> request) where T : ShardData
        {
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

            return await _node.Handle(new AddShardWriteOperation()
            {
                Data = request.Data,
                WaitForSafeWrite = request.WaitForSafeWrite,
                Operation = request.Operation,
                RemoveLock = request.RemoveLock,
                LockId = request.LockId
            });
        }
    }
}
