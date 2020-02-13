using Cindi.Application.Interfaces;
using Cindi.Domain.Enums;
using ConsensusCore.Domain.BaseClasses;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Persistence
{
    public class EntityRepository<T> : IEntityRepository<T> where T : class
    {
        public string DatabaseName { get; } = "CindiDb";
        public IMongoCollection<T> _entity;

        public EntityRepository(string mongoDbConnectionString, string databaseName)
        {
            DatabaseName = databaseName;
            var client = new MongoClient(mongoDbConnectionString);
            SetCollection(client);
        }

        public EntityRepository(IMongoClient client)
        {
            SetCollection(client);
        }

        private void SetCollection(IMongoClient client)
        {
            var database = client.GetDatabase(DatabaseName);
            _entity = database.GetCollection<T>(typeof(T).Name);
        }

        public async Task<IEnumerable<T>> GetAsync<TMember>(Expression<Func<T, bool>> expression = null, List<Expression<Func<T, object>>> exclusions = null, Expression<Func<T, TMember>> sortBy = null, SortOrder order = SortOrder.Descending, int size = 10, int page = 0)
        {
            SortDefinition<T> sort = null;

            if (sortBy != null)
            {
                if (order == SortOrder.Descending)
                {
                    sort = Builders<T>.Sort.Descending(typeof(TMember).Name);
                }
                else
                {
                    sort = Builders<T>.Sort.Ascending(typeof(TMember).Name);
                }
            }

            FindOptions<T> options = new FindOptions<T>
            {
                BatchSize = size,
                NoCursorTimeout = false,
                Skip = page * size,
                Limit = size,
                Sort = sort
            };

            if (exclusions != null)
            {
                foreach (var exclusion in exclusions)
                {
                    options.Projection = Builders<T>.Projection.Exclude(exclusion);
                }
            }

            if (expression != null)
                return (await _entity.FindAsync(expression, options)).ToEnumerable();
            else
                return (await _entity.FindAsync(FilterDefinition<T>.Empty, options)).ToEnumerable();
        }

        public async Task<T> Insert(T entity)
        {
            await _entity.InsertOneAsync(entity);
            return entity;
        }

        public async Task<T> Update(Expression<Func<T, bool>> expression, T entity, bool isUpsert = false)
        {
            var result = await _entity.ReplaceOneAsync(
                  expression,
                  entity,
                  new UpdateOptions
                  {
                      IsUpsert = isUpsert
                  });

            if (result.IsAcknowledged)
            {
                return entity;
            }
            else
            {
                throw new Exception("Update on collection " + typeof(T).Name + " failed.");
            }
        }
    }
}
