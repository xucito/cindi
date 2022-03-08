using Cindi.Domain.Entities;
using Elasticsearch.Net.Specification.FleetApi;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Utilities
{
    public static class ElasticClientExtensions
    {
        public static async Task<Guid?> LockObject<T>(this ElasticClient client, Guid i, int lockTimeoutMs = 60000) where T:BaseEntity
        {
            var lockId = Guid.NewGuid();
            //var result = await client.UpdateByQueryAsync<T>(u => u.Query(q => q.Term(f => f.Id, i) && (!q.Exists(t => t.Field(f => f.LockId)) || q.DateRange(f => f.Field(a => a.LockExpiresOn).LessThan(DateTime.Now)))).Script($"ctx._source.lockTimeoutMs={lockTimeoutMs}; ctx._source.lockId='{lockId}'; ctx._source.lockCreatedOn='{DateTimeOffset.UtcNow.ToString("o")}';ctx._source.lockExpiresOn='{(DateTimeOffset.UtcNow.AddMilliseconds(lockTimeoutMs)).ToString("o")}';").Conflicts(Elasticsearch.Net.Conflicts.Abort).Refresh());

            //var result = await client.UpdateAsync<T, object>(i, u => u.Doc(new
            //{
            //    LockId = lockId,
            //    LockCreatedOn = DateTime.UtcNow,
            //    LockExpiresOn = DateTimeOffset.UtcNow.AddMilliseconds(lockTimeoutMs),
            //    LockTimeoutMs = lockTimeoutMs
            //}));
            var result = await client.UpdateByQueryAsync<T>(u => u.Query(q => q.Term(f => f.Id, i) && (!q.Exists(t => t.Field(f => f.LockId)) || q.DateRange(f => f.Field(a => a.LockExpiresOn).LessThan(DateTime.Now)))).Script(s => s.Id("apply-lock").Params(new Dictionary<string, object>()
            {
                {"lockTimeoutMs",lockTimeoutMs },
                {"lockid", lockId },
                {"lockCreatedOn",DateTimeOffset.UtcNow.ToString("o") },
                {"lockExpiresOn",(DateTimeOffset.UtcNow.AddMilliseconds(lockTimeoutMs)).ToString("o") },
            })).Conflicts(Elasticsearch.Net.Conflicts.Abort).Refresh());

            if (result.IsValid && result.Updated > 0)
            {
                var searchedObject = await FirstOrDefaultAsync<T>(client, i);

                if(searchedObject != null && searchedObject.LockId == lockId)
                {
                    return lockId;
                }
            }

            return null;
        }

        public static async Task<T> LockAndGetObject<T>(this ElasticClient client, Guid id, int lockTimeoutMs = 60000) where T : BaseEntity
        {
            var lockId = await LockObject<T>(client, id, lockTimeoutMs);

            if (lockId != null)
            {
                return await FirstOrDefaultAsync<T>(client, id);
            }
            else
            {
                return null;
            }
        }

        public static async Task<TDocument> LockAndGetObject<TDocument>(this ElasticClient client, Func<SearchDescriptor<TDocument>, ISearchRequest> selector = null, CancellationToken ct = default(CancellationToken), int lockTimeoutMs = 60000) where TDocument : BaseEntity, new()
        {
            var searchedObject = await FirstOrDefaultAsync<TDocument>(client, selector);

            if (searchedObject != null)
            {
                var lockId = await LockObject<TDocument>(client, searchedObject.Id, lockTimeoutMs);

                if (lockId != null)
                {
                    return searchedObject;
                }
            }
            return null;
        }

        public static async Task<bool> Unlock<T>(this ElasticClient client, Guid id, int lockTimeoutMs = 60000) where T : BaseEntity
        {
            //var result = await client.UpdateByQueryAsync<T>(u => u.Query(q => q.Term(f => f.Id, id) && q.Exists(t => t.Field(f => f.LockId))).Script($"ctx._source.lockTimeoutMs=null; ctx._source.lockId=null; ctx._source.lockCreatedOn=null;ctx._source.lockExpiresOn=null;").Conflicts(Elasticsearch.Net.Conflicts.Abort).Refresh());
            var result = await client.UpdateAsync<T, object>(id, u => u.Doc(new
            {
                LockId = (string)null,
                LockCreatedOn = (string)null,
                LockExpiresOn = (string)null,
                LockTimeoutMs = (string)null
            }));

            return result.IsValid;
        }

        public static async Task<Guid?> LockObject<T>(this ElasticClient client, T toLock, int lockTimeoutMs = 60000) where T : BaseEntity, new()
        {
            return await LockObject<T>(client, toLock.Id, lockTimeoutMs);
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this ElasticClient client, Guid Id) where T : BaseEntity
        {
            return (await client.SearchAsync<T>(a => a.Query(q => q.Term(o => o.Id, Id)))).Hits.FirstOrDefault()?.Source;
        }

        public static bool Create<T>(this ElasticClient client) where T : BaseEntity
        {
            var name = typeof(T).Name;
            var index = client.Indices.Create("swarm-" + typeof(T).Name.ToLower() + "s", c => c
                   .Map<T>(m => m.AutoMap<T>()));
            return index.IsValid;
        }

        public static async Task<TDocument> FirstOrDefaultAsync<TDocument>(this ElasticClient client, Func<SearchDescriptor<TDocument>, ISearchRequest> selector = null, CancellationToken ct = default(CancellationToken)) where TDocument : class
        {
            var search = await client.SearchAsync<TDocument>(selector);
            return (await client.SearchAsync<TDocument>(selector)).Hits.FirstOrDefault()?.Source;
        }

        public static async Task<TDocument> FirstOrDefaultAsync<TDocument>(this ElasticClient client, QueryContainer selector = null, CancellationToken ct = default(CancellationToken)) where TDocument : class
        {
            return (await client.SearchAsync<TDocument>(d => d.Query(q => selector).Size(1))).Hits.FirstOrDefault()?.Source;
        }

        public static async Task<bool> MakeSureIsQueryable<T>(this ElasticClient client, Guid Id) where T : BaseEntity
        {
            var counter = 0;
            T entity = null;
            while ((entity = await client.FirstOrDefaultAsync<T>(Id)) == null && counter < 3)
            {
                counter++;
                await Task.Delay(1000);
            }

            if(entity == null)
            {
                throw new Exception("Failed to query object " + Id.ToString());
            }
            return true;
        }
    }

    public static class SearchDescriptor
    {
        public static SearchDescriptor<T> Sort<T>(this SearchDescriptor<T> client, string sort) where T : class
        {
            var sd = new SortDescriptor<T>();
            sd.Field(new Field(sort.Split(":")[0]), sort.Split(":")[1] == "0" ? SortOrder.Ascending : SortOrder.Descending);
            client.Sort(s => sd);
            return client;
        }

    }

    public static class ConnectionSettingsBase
    {
        public static ConnectionSettings DefaultMappingFor<T>(this ConnectionSettings baseConnectionSetting) where T : class
        {
            return baseConnectionSetting.DefaultMappingFor<T>(d => d.IndexName("swarm-" + typeof(T).Name.ToLower() + "s"));
        }
    }
}
