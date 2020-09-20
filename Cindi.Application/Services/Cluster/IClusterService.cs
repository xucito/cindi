using ConsensusCore.Domain.BaseClasses;
using ConsensusCore.Domain.RPCs.Shard;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Cindi.Application.Services.ClusterOperation
{
    public interface IClusterService
    {
        Task<AddShardWriteOperationResponse> AddWriteOperation<T>(EntityWriteOperation<T> request) where T : ShardData;
        long Count<T>(Expression<Func<T, bool>> expression = null);
        Task<IEnumerable<T>> GetAsync<T>(Expression<Func<T, bool>> expression = null, List<Expression<Func<T, object>>> exclusions = null, string sort = null, int size = 10, int page = 0);
        Task<T> GetFirstOrDefaultAsync<T>(Expression<Func<T, bool>> expression);
        Task<TResponse> Handle<TResponse>(IClusterRequest<TResponse> request) where TResponse : BaseResponse, new();
    }
}