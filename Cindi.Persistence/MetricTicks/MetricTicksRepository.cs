using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.Metrics;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Persistence.MetricTicks
{
    public class MetricTicksRepository : BaseRepository, IMetricTicksRepository
    {
        private IMongoCollection<MetricTick> _metricTicks;

        public MetricTicksRepository(string mongoDbConnectionString, string databaseName) : base(mongoDbConnectionString, databaseName)
        {
            var client = new MongoClient(mongoDbConnectionString);
            SetCollection(client);
        }

        public MetricTicksRepository(IMongoClient client) : base(client)
        {
            SetCollection(client);
        }

        private void SetCollection(IMongoClient client)
        {
            var database = client.GetDatabase(DatabaseName);
            _metricTicks = database.GetCollection<MetricTick>("MetricTicks");
        }

        public async Task<Dictionary<int, SortedDictionary<DateTime, MetricTick>>> GetMetricTicksAsync(string objectId, DateTime fromDate, int[] metricIds)
        {
            /*
            var builder = Builders<Metric>.Filter;
            var filters = new List<FilterDefinition<Metric>>();
            var keysFilter = FilterDefinition<Metric>.Empty;
            FindOptions<Metric> options = new FindOptions<Metric>
            {
                BatchSize = size,
                NoCursorTimeout = false,
                Skip = page,
                Limit = size
            };

            var metrics = (await _metrics.FindAsync(keysFilter, options)).ToList();

            return metrics;
        */
            return null;
        }

        public async Task<MetricTick> InsertMetricTicksAsync(MetricTick metricTick)
        {
            try
            {
                await _metricTicks.InsertOneAsync(metricTick);
                return metricTick;
            }
            catch (MongoDB.Driver.MongoWriteException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
            return null;
        }

        public async Task<bool> DeleteMetricTicks(string ObjectId, DateTime toDate, int metricId)
        {
            /*
            var result = await _keys.DeleteOneAsync(u => u.Id == id);

            if (result.IsAcknowledged)
            {
                return true;
            }*/
            return false;
        }

        public async Task<DateTime?> GetLastMetricTickDate(int metricId)
        {
            var tick = (await _metricTicks.FindAsync(mt => mt.MetricId == metricId, new FindOptions<MetricTick>()
            {
                Sort = Builders<MetricTick>.Sort.Descending(mt => mt.Date),
                Limit = 1
            })).FirstOrDefault();

            if (tick == null)
                return null;
            return tick.Date;
        }

        public async Task<object> GetMetricTicksAsync(DateTime fromDate, DateTime toDate, int metricId, string[] aggs, char interval = 'S', string subcategory = null, Guid? objectId = null, bool includeSubcategories = false)
        {

            var dateString = "";

            switch(interval)
            {
                case 'Y':
                    dateString = "%Y-01-01T00:00:00.000Z";
                    break;
                case 'm':
                    dateString = "%Y-%m-01T00:00:00.000Z";
                    break;
                case 'd':
                    dateString = "%Y-%m-%dT00:00:00.000Z";
                    break;
                case 'H':
                    dateString = "%Y-%m-%dT%H:00:00.000Z";
                    break;
                case 'M':
                    dateString = "%Y-%m-%dT%H:%M:00.000Z";
                    break;
                case 'S':
                    dateString = "%Y-%m-%dT%H:%M:%S.000Z";
                    break;
            }

            var filterDoc = new BsonDocument{{"date", new BsonDocument(){
                        { "$dateToString", new BsonDocument { { "format", dateString }, { "date", "$Date"} } },
                    } } };

            var aggsDoc = new BsonDocument();

            foreach (var agg in aggs)
            {
                aggsDoc[agg] = new BsonDocument { { "$" + agg, "$Value" } };
            }

            var idDocument = filterDoc;
            if (includeSubcategories)
            {
                idDocument["subcategory"] = "$SubCategory";
            }
            var tick = await _metricTicks.Aggregate()
                .Match(mt => mt.MetricId == metricId && mt.Date >= fromDate && mt.Date <= toDate)
                //.Match(mt => metricIdsAndSubcategories.Contains(mt.MetricId + (mt.SubCategory != null ? "_" + mt.SubCategory : "")))
                .Group(new BsonDocument{ {"_id", idDocument },
            { aggsDoc
                } })
                .Sort(new BsonDocument { { "_id.date", -1 } })
                .ToListAsync();

            return JsonConvert.DeserializeObject<object>(tick.ToJson());
        }
    }
}
