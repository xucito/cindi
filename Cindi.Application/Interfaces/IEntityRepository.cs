using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cindi.Domain.Enums;

namespace Cindi.Application.Interfaces
{
    public interface IEntitiesRepository
    {
        Task<IEnumerable<T>> GetAsync<T>(Expression<Func<T, bool>> expression = null, List<Expression<Func<T, object>>> exclusions = null, string sort = null, int size = 10, int page = 0);
        Task<T> GetFirstOrDefaultAsync<T>(Expression<Func<T, bool>> expression);
        Task<T> Insert<T>(T entity);
        Task<T> Update<T>(T entity);
        long Count<T>(Expression<Func<T, bool>> expression = null);
        Task<bool> Delete<T>(Expression<Func<T, bool>> expression);
        /// <summary>
        /// Used to rebuild the database and size.
        /// </summary>
        void Rebuild();
    }
}