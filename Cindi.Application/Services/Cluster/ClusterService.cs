using Cindi.Application.Interfaces;
using Cindi.Domain.Entities;
using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Node.Communication.Controllers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Services.ClusterOperation
{
    public class ClusterService : IClusterService
    {
        IClusterRequestHandler _node;
        ILogger _logger;
        IEntitiesRepository _entitiesRepository;
        public ClusterService(IClusterRequestHandler node,
            ILogger<ClusterService> logger,
            IEntitiesRepository entitiesRepository)
        {
            _node = node;
            _logger = logger;
            _entitiesRepository = entitiesRepository;
        }

        public async Task<AddShardWriteOperationResponse> AddWriteOperation<T>(EntityWriteOperation<T> request) where T : ShardData
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

        public long Count<T>(Expression<Func<T, bool>> expression = null)
        {
            return _entitiesRepository.Count(expression);
        }

        public async Task<IEnumerable<T>> GetAsync<T>(Expression<Func<T, bool>> expression = null, List<Expression<Func<T, object>>> exclusions = null, string sort = null, int size = 10, int page = 0)
        {
            return await _entitiesRepository.GetAsync(expression, exclusions, sort, size, page);
        }

        public async Task<T> GetFirstOrDefaultAsync<T>(Expression<Func<T, bool>> expression)
        {
            return await _entitiesRepository.GetFirstOrDefaultAsync(expression);
        }

        public async Task<TResponse> Handle<TResponse>(IClusterRequest<TResponse> request) where TResponse : BaseResponse, new()
        {
            return await _node.Handle(request);
        }
    }
}
