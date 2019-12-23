using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.Metrics;
using MongoDB.Driver;
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
    }
}
