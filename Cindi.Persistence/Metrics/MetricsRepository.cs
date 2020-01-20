using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.Metrics;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cindi.Persistence.Metrics
{
    public class MetricsRepository : BaseRepository, IMetricsRepository
    {
        private IMongoCollection<Metric> _metrics;

        public MetricsRepository(string mongoDbConnectionString, string databaseName) : base(mongoDbConnectionString, databaseName)
        {
            var client = new MongoClient(mongoDbConnectionString);
            SetCollection(client);
        }

        public MetricsRepository(IMongoClient client) : base(client)
        {
            SetCollection(client);
        }

        private void SetCollection(IMongoClient client)
        {
            var database = client.GetDatabase(DatabaseName);
            _metrics = database.GetCollection<Metric>("Metrics");
        }

        public async Task<IEnumerable<Metric>> GetMetricsAsync(int size = 10, int page = 0)
        {
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
        }


        public async Task<Metric> GetMetricAsync(int metricId)
        {
            var step = (await _metrics.FindAsync(s => s.MetricId == metricId)).FirstOrDefault();
            return step;
        }


        public async Task<Metric> InsertMetricsAsync(Metric metrics)
        {
            try
            {
                await _metrics.InsertOneAsync(metrics);
                return metrics;
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
        }

        public async Task<Metric> GetMetricAsync(string metricName)
        {
            return (await _metrics.FindAsync(m => m.MetricName == metricName)).FirstOrDefault();
        }
    }
}
