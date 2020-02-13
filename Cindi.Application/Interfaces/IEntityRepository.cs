using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cindi.Domain.Enums;

namespace Cindi.Application.Interfaces
{
    public interface IEntityRepository<T> where T : class
    {
        string DatabaseName { get; }

        Task<IEnumerable<T>> GetAsync<TMember>(Expression<Func<T, bool>> expression = null, List<Expression<Func<T, object>>> exclusions = null, Expression<Func<T, TMember>> sortBy = null, SortOrder order = SortOrder.Descending, int size = 10, int page = 0);
        Task<T> Insert(T entity);
        Task<T> Update(Expression<Func<T, bool>> expression, T entity, bool isUpsert = false);
    }
}