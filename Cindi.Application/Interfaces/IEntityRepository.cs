using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cindi.Domain.Enums;

namespace Cindi.Application.Interfaces
{
    public interface IEntityRepository
    {
        string DatabaseName { get; }
        Task<IEnumerable<T>> GetAsync<T>(Expression<Func<T, bool>> expression = null, List<Expression<Func<T, object>>> exclusions = null, string sort = null, int size = 10, int page = 0);
        Task<T> GetFirstOrDefaultAsync<T>(Expression<Func<T, bool>> expression);
        Task<T> Insert<T>(T entity);
        Task<T> Update<T>(Expression<Func<T, bool>> expression, T entity, bool isUpsert = false);
        long Count<T>(Expression<Func<T, bool>> expression = null);
        Task<bool> Delete<T>(Expression<Func<T, bool>> expression);
    }
}