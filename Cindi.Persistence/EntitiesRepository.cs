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
        LiteDatabase db;
        ConcurrentDictionary<string, string> keyDict = new ConcurrentDictionary<string, string>();

        public EntitiesRepository(string databaseLocation)
        {

            _databaseLocation = databaseLocation;
            db = new LiteDatabase(_databaseLocation);
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
            swo.EnsureIndex(o => o.Data.ShardId);
            swo.EnsureIndex(o => o.Pos);
            swo.EnsureIndex(o => o.Id);
            swo.EnsureIndex(o => o.Operation);
            var user = db.GetCollection<User>(NormalizeCollectionString(typeof(User)));
            user.EnsureIndex(u => u.Username);
            var step = db.GetCollection<Step>(NormalizeCollectionString(typeof(Step)));
            step.EnsureIndex(u => u.Status);
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

            return collection.Count(expression);
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
                result = collection.Find(expression).Skip(page * size).Take(size);
            }
            else
            {
                result = collection.Find(Query.All()).Skip(page * size).Take(size);
            }


            //Console.WriteLine("GetAsync took " + stopwatch.ElapsedMilliseconds);
            return result;
        }

        public async Task<bool> Delete<T>(Expression<Func<T, bool>> expression)
        {
            //var stopwatch = new Stopwatch();
            //stopwatch.Start();

            var collection = db.GetCollection<T>(NormalizeCollectionString(typeof(T))); ;
            collection.DeleteMany(expression);
            //Console.WriteLine("Delete took " + stopwatch.ElapsedMilliseconds);
            return true;
        }

        public async Task<T> Insert<T>(T entity)
        {
            //var stopwatch = new Stopwatch();
            //stopwatch.Start();
            var collection = db.GetCollection<T>(NormalizeCollectionString(typeof(T))); ;
            collection.Insert(entity);
            //Console.WriteLine("Insert took " + stopwatch.ElapsedMilliseconds);
            return await Task.FromResult(entity);
        }

        public async Task<T> Update<T>(T entity)
        {
            //var stopwatch = new Stopwatch();
            //stopwatch.Start();

            var collection = db.GetCollection<T>(NormalizeCollectionString(typeof(T))); ;
            collection.Update(entity);
            //Console.WriteLine("Update took " + stopwatch.ElapsedMilliseconds);
            return await Task.FromResult(entity);
        }

        public void Update<T>(Expression<Func<T, T>> predicate, Expression<Func<T, bool>> expression)
        {
            //var stopwatch = new Stopwatch();
            //stopwatch.Start();

            var collection = db.GetCollection<T>(NormalizeCollectionString(typeof(T))); ;
            //Console.WriteLine("Update took " + stopwatch.ElapsedMilliseconds);
            collection.UpdateMany(predicate, expression);
        }

        public async Task<T> GetFirstOrDefaultAsync<T>(Expression<Func<T, bool>> expression)
        {
            //var stopwatch = new Stopwatch();
            //stopwatch.Start();
            var collection = db.GetCollection<T>(NormalizeCollectionString(typeof(T)));
            //Console.WriteLine("GetFirst took " + stopwatch.ElapsedMilliseconds);
            return collection.FindOne(expression);
        }

        public ILiteCollection<T> GetCollection<T>()
        {
            return db.GetCollection<T>(NormalizeCollectionString(typeof(T)));
        }
    }
}
