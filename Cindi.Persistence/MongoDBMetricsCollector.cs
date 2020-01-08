using Cindi.Application.Interfaces;
using Cindi.Application.SharedValues;
using Cindi.Domain.Entities.Metrics;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Persistence
{
    public class MongoDBMetricsCollector : IDatabaseMetricsCollector
    {
        IMongoClient _client;
        IMongoDatabase _db;
        DateTime? lastPolled;
        ConcurrentDictionary<string, double> lastLatencies = new ConcurrentDictionary<string, double>();
        ConcurrentDictionary<string, double> lastOps = new ConcurrentDictionary<string, double>();

        public MongoDBMetricsCollector(string mongoDbConnectionString, string databaseName)
        {
            var client = new MongoClient(mongoDbConnectionString);
            _db = client.GetDatabase(databaseName);
        }

        public MongoDBMetricsCollector(IMongoClient client)
        {
            _client = client;
            _db = client.GetDatabase("CindiDb");
        }

        public async Task<List<MetricTick>> GetMetricsAsync(Guid nodeId)
        {
            var result = await _db.RunCommandAsync(new BsonDocumentCommand<BsonDocument>(new BsonDocument { { "serverStatus", 1 } }));

            var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
            var jObjectConverted = JObject.Parse(result.ToJson(jsonWriterSettings));

            List<MetricTick> metrics = new List<MetricTick>();

            var pollTime = result["localTime"].ToUniversalTime();
            var latencies = jObjectConverted["opLatencies"].ToObject<JObject>();

            if (lastPolled != null)
            {
                var timeDifference = (pollTime - lastPolled.Value).TotalMilliseconds;
            }

            foreach (JProperty x in (JToken)latencies)
            { // if 'obj' is a JObject
                string name = x.Name;
                JToken value = x.Value;

                if (!lastLatencies.ContainsKey(x.Name))
                {
                    lastLatencies.TryAdd(x.Name, 0);
                    lastOps.TryAdd(x.Name, 0);
                }

                if (lastPolled != null)
                {
                    var changeInOps = (x.Value["ops"].ToObject<double>() - lastOps[x.Name]);
                    metrics.Add(new MetricTick()
                    {
                        MetricId = MetricLibrary.DatabaseOperationLatencyMs.MetricId,
                        Date = pollTime,
                        ObjectId = nodeId,
                        Value = changeInOps  != 0 ?((x.Value["latency"].ToObject<double>() - lastLatencies[x.Name]) / (x.Value["ops"].ToObject<double>() - lastOps[x.Name])) / 1000 : 0,
                        SubCategory = name
                    });

                    metrics.Add(new MetricTick()
                    {
                        MetricId = MetricLibrary.DatabaseOperationCount.MetricId,
                        Date = pollTime,
                        ObjectId = nodeId,
                        Value = (x.Value["ops"].ToObject<double>() - lastOps[x.Name]),
                        SubCategory = name
                    });
                }

                lastLatencies[x.Name] = x.Value["latency"].ToObject<double>();
                lastOps[x.Name] = x.Value["ops"].ToObject<double>();
            }

            lastPolled = pollTime;

            return metrics;
        }
    }
}
