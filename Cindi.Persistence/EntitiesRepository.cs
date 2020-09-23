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
using Cindi.Domain.Entities.States;
using ConsensusCore.Domain.Services;
using Newtonsoft.Json;
using System.Diagnostics;
using Cindi.Domain.Entities.Users;
using LiteDB.Async;

namespace Cindi.Persistence
{
    public class EntitiesRepository : IEntitiesRepository
    {
        private string _databaseLocation;
        LiteDatabaseAsync db;
        ConcurrentDictionary<string, string> keyDict = new ConcurrentDictionary<string, string>();

        public EntitiesRepository(string databaseLocation)
        {

            _databaseLocation = databaseLocation;
            db = new LiteDatabaseAsync(_databaseLocation);
        }

        public void Setup()
        {
            var mapper = BsonMapper.Global;
            /*mapper.Entity<NodeStorage<CindiClusterState>>()
                .Ignore(x => x.CommandsQueue);*/
            BsonMapper.Global.RegisterType<CindiClusterState>(a =>
            {
                return JsonConvert.SerializeObject(a);
            }, b =>
            {
                return JsonConvert.DeserializeObject<CindiClusterState>(b.AsString);
            });

            var swo = db.GetCollection<ShardWriteOperation>(NormalizeCollectionString(typeof(ShardWriteOperation)));
            swo.EnsureIndexAsync(o => o.Data.ShardId).GetAwaiter().GetResult();
            swo.EnsureIndexAsync(o => o.Pos).GetAwaiter().GetResult();
            swo.EnsureIndexAsync(o => o.Id).GetAwaiter().GetResult();
            swo.EnsureIndexAsync(o => o.Operation);
            var user = db.GetCollection<User>(NormalizeCollectionString(typeof(User)));
            user.EnsureIndexAsync(u => u.Username).GetAwaiter().GetResult();
            var step = db.GetCollection<Step>(NormalizeCollectionString(typeof(Step)));
            step.EnsureIndexAsync(u => u.Status).GetAwaiter().GetResult();
        }

        public string NormalizeCollectionString(Type type)
        {
            if (!keyDict.ContainsKey(type.Name))
            {
                keyDict.TryAdd(type.Name, Regex.Replace(type.Name, @"[^0-9a-zA-Z:,]+", "_"));
            }

            return keyDict[type.Name];
        }

        public long Count<T>(Expression<Func<T, bool>> expression = null)
        {
            //var stopwatch = new Stopwatch();
            //stopwatch.Start();

            var collection = db.GetCollection<T>(NormalizeCollectionString(typeof(T)));

            //Console.WriteLine("Count took " + stopwatch.ElapsedMilliseconds);

            return collection.CountAsync(expression).GetAwaiter().GetResult();
        }

        public async Task<IEnumerable<T>> GetAsync<T>(Expression<Func<T, bool>> expression = null, List<Expression<Func<T, object>>> exclusions = null, string sort = null, int size = 10000, int page = 0)
        {
            //var stopwatch = new Stopwatch();
            //stopwatch.Start();

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

            IEnumerable<T> result;
            if (expression != null)
            {
                result = await collection.FindAsync(expression, page * size, size);
            }
            else
            {
                result = await collection.FindAsync(Query.All(), page * size, size);
            }


            //Console.WriteLine("GetAsync took " + stopwatch.ElapsedMilliseconds);
            return result;
        }

        public async Task<bool> Delete<T>(Expression<Func<T, bool>> expression)
        {
            //var stopwatch = new Stopwatch();
            //stopwatch.Start();

            var collection = db.GetCollection<T>(NormalizeCollectionString(typeof(T))); ;
            await collection.DeleteManyAsync(expression);
            //Console.WriteLine("Delete took " + stopwatch.ElapsedMilliseconds);
            return true;
        }

        public async Task<T> Insert<T>(T entity)
        {
            //var stopwatch = new Stopwatch();
            //stopwatch.Start();
            var collection = db.GetCollection<T>(NormalizeCollectionString(typeof(T))); ;
            await collection.InsertAsync(entity);
            //Console.WriteLine("Insert took " + stopwatch.ElapsedMilliseconds);
            return await Task.FromResult(entity);
        }

        public async Task<T> Update<T>(T entity)
        {
            //var stopwatch = new Stopwatch();
            //stopwatch.Start();

            var collection = db.GetCollection<T>(NormalizeCollectionString(typeof(T))); ;
            await collection.UpdateAsync(entity);
            //Console.WriteLine("Update took " + stopwatch.ElapsedMilliseconds);
            return await Task.FromResult(entity);
        }

        public async Task<T> GetFirstOrDefaultAsync<T>(Expression<Func<T, bool>> expression = null)
        {
            //var stopwatch = new Stopwatch();
            //stopwatch.Start();
            var collection = db.GetCollection<T>(NormalizeCollectionString(typeof(T)));
            //Console.WriteLine("GetFirst took " + stopwatch.ElapsedMilliseconds);
            if(expression == null)
            {
                return await collection.FindOneAsync(Query.All());
            }
            return await collection.FindOneAsync(expression);
        }

        public LiteCollectionAsync<T> GetCollection<T>()
        {
            return db.GetCollection<T>(NormalizeCollectionString(typeof(T)));
        }
    }
}
