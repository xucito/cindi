using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Enums;
using ConsensusCore.Domain.BaseClasses;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace Cindi.Persistence
{
    public class EntitiesRepository : IEntitiesRepository
    {
        private string _databaseLocation;
        LiteDatabase db;
        ConcurrentDictionary<string, string> keyDict = new ConcurrentDictionary<string, string>();

        public EntitiesRepository(string databaseLocation)
        {
            var mapper = BsonMapper.Global;

            _databaseLocation = databaseLocation;
            db = new LiteDatabase(_databaseLocation);
        }

        public string NormalizeCollectionString(Type type)
        {
            if(!keyDict.ContainsKey(type.Name))
            {
                keyDict.TryAdd(type.Name, Regex.Replace(type.Name, @"[^0-9a-zA-Z:,]+", "_"));
            }

            return keyDict[type.Name];
        }

        public long Count<T>(Expression<Func<T, bool>> expression = null)
        {
            var collection = db.GetCollection<T>(NormalizeCollectionString(typeof(T))); ;
            return collection.Count(expression);
        }

        public async Task<IEnumerable<T>> GetAsync<T>(Expression<Func<T, bool>> expression = null, List<Expression<Func<T, object>>> exclusions = null, string sort = null, int size = 10000, int page = 0)
        {
            Query sortQuery;
            var transformedSortString = "";
            if (sort != null)
            {
                var split = sort.Split(",")[0];
                sortQuery = Query.All(split.Split(':')[0], int.Parse(split.Split(':')[1]));
            }
            else
            {
                sortQuery = Query.All();
            }

            var collection = db.GetCollection<T>(NormalizeCollectionString(typeof(T)));

            if (expression != null)
            {
                return collection.Find(expression).Skip(page * size).Take(size);
            }
            else
            {
                return collection.Find(Query.All()).Skip(page * size).Take(size);
            }
        }

        public async Task<bool> Delete<T>(Expression<Func<T, bool>> expression)
        {
            var collection = db.GetCollection<T>(NormalizeCollectionString(typeof(T))); ;
            collection.DeleteMany(expression);
            return true;
        }

        public async Task<T> Insert<T>(T entity)
        {
            var collection = db.GetCollection<T>(NormalizeCollectionString(typeof(T))); ;
            collection.Insert(entity);
            return await Task.FromResult(entity);
        }

        public async Task<T> Update<T>(T entity)
        {
            var collection = db.GetCollection<T>(NormalizeCollectionString(typeof(T))); ;
            collection.Update(entity);
            return await Task.FromResult(entity);
        }

        public void Update<T>(Expression<Func<T, T>> predicate, Expression<Func<T, bool>> expression)
        {
            var collection = db.GetCollection<T>(NormalizeCollectionString(typeof(T))); ;
            collection.UpdateMany(predicate, expression);
        }

        public async Task<T> GetFirstOrDefaultAsync<T>(Expression<Func<T, bool>> expression)
        {
            var collection = db.GetCollection<T>(NormalizeCollectionString(typeof(T))); ;
            return collection.FindOne(expression);
        }
    }
}
