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
        public static async Task<T> LockObject<T>(this ElasticClient client, Guid i)
        {
            return default(T);
        }

        public static async Task<T> LockObject<T>(this ElasticClient client, T toLock)
        {
            return default(T);
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
            return (await client.SearchAsync<TDocument>(selector)).Hits.FirstOrDefault()?.Source;
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
        public static ConnectionSettings DefaultMappingFor<T>(this ConnectionSettings baseConnectionSetting) where T:class
        {
           return baseConnectionSetting.DefaultMappingFor<T>(d => d.IndexName("swarm-" + typeof(T).Name.ToLower() + "s"));
        }
    }
}
