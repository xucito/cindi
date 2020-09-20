using Cindi.Application.Interfaces;
using Cindi.Domain.Entities.Metrics;
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

        public MetricTicksRepository(string databaseLocation) : base(databaseLocation)
        {
            //var client = new MongoClient(mongoDbConnectionString);
            //SetCollection(client);
        }

        public async Task<MetricTick> InsertMetricTicksAsync(MetricTick metricTick)
        {
            return null;
        }

        public async Task<DateTime?> GetLastMetricTickDate(int metricId)
        {
            return null;
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

            return null;
        }
    }
}
