using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Enums;
using ConsensusCore.Domain.BaseClasses;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Persistence
{
    public class EntityRepository : IEntityRepository
    {
        public string DatabaseName { get; } = "CindiDb";
        private IMongoClient _client;
        private IMongoDatabase _database;

        public EntityRepository(string mongoDbConnectionString, string databaseName)
        {
            DatabaseName = databaseName;
            var client = new MongoClient(mongoDbConnectionString);
            _database = client.GetDatabase(DatabaseName);
        }

        public EntityRepository(IMongoClient client)
        {
            _database = client.GetDatabase(DatabaseName);
        }

        public long Count<T>(Expression<Func<T, bool>> expression = null)
        {
            IMongoCollection<T> _entity = GetDatabaseCollection<T>();
            if (expression == null)
            {
                return _entity.EstimatedDocumentCount();
            }
            return _entity.Find(expression).CountDocuments();
        }

        public IMongoCollection<T> GetDatabaseCollection<T>()
        {
            return _database.GetCollection<T>(EntitiesTableMap.GetTableName<T>());
        }

        public async Task<IEnumerable<T>> GetAsync<T>(Expression<Func<T, bool>> expression = null, List<Expression<Func<T, object>>> exclusions = null, string sort = null, int size = 10, int page = 0)
        {
            IMongoCollection<T> _entity = GetDatabaseCollection<T>();
            ProjectionDefinition<T> definition = Builders<T>.Projection.Exclude("");
            if (exclusions != null)
            {
                var first = true;
                foreach (var exclusion in exclusions)
                {
                    if (first)
                    {
                        first = false;
                        definition = Builders<T>.Projection.Exclude(exclusion);
                    }
                    else
                        definition.Exclude(exclusion);
                }
            }

            var transformedSortString = "";

            if (sort != null)
            {
                foreach (var sortStatement in sort.Split(","))
                {
                    if (transformedSortString != "")
                    {
                        transformedSortString += ",";
                    }
                    transformedSortString += typeof(T).GetProperty(sortStatement.Split(":")[0], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).Name + ": " + sortStatement.Split(":")[1];
                }
            }

            return await _entity.Find(expression != null ? expression : FilterDefinition<T>.Empty)
                .Sort(sort != null ? "{" + transformedSortString + "}" : null)
                .Limit(size).Skip(page * size)
                .Project<T>(definition)
                .ToListAsync<T>();
        }

        public async Task<bool> Delete<T>(Expression<Func<T, bool>> expression)
        {
            IMongoCollection<T> _entity = GetDatabaseCollection<T>();
            var result = await _entity.DeleteOneAsync(expression);
            if (result.IsAcknowledged)
            {
                return true;
            }
            return false;
        }

        public async Task<T> Insert<T>(T entity)
        {
            IMongoCollection<T> _entity = GetDatabaseCollection<T>();
            await _entity.InsertOneAsync(entity);
            return entity;
        }

        public async Task<T> Update<T>(Expression<Func<T, bool>> expression, T entity, bool isUpsert = false)
        {
            IMongoCollection<T> _entity = GetDatabaseCollection<T>();
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

        public async Task<T> GetFirstOrDefaultAsync<T>(Expression<Func<T, bool>> expression)
        {
            IMongoCollection<T> _entity = GetDatabaseCollection<T>();
            return (await _entity.FindAsync(expression)).FirstOrDefault();
        }
    }
}
