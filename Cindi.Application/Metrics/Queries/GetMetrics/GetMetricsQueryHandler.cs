using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services;
using Cindi.Domain.Entities.Metrics;
using Cindi.Domain.Entities.States;

using MediatR;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Metrics.Queries.GetMetrics
{
    public class GetMetricsQueryHandler : IRequestHandler<GetMetricsQuery, QueryResult<object>>
    {
        ILogger<GetMetricsQueryHandler> _logger;
        IEntitiesRepository _entitiesRepository;

        public GetMetricsQueryHandler(ILogger<GetMetricsQueryHandler> logger,
            IEntitiesRepository entitiesRepository)
        {
            _entitiesRepository = entitiesRepository;
            _logger = logger;
        }

        public async Task<QueryResult<object>> Handle(GetMetricsQuery request, CancellationToken cancellationToken)
        {
            var records = request.Metrics.Select(kv => kv.Key).ToArray();
            var foundMetrics = (await _entitiesRepository.GetAsync<Metric>(m => records.Contains(m.MetricName)));

            var ids = foundMetrics.Select(fm => fm.MetricId).ToArray();

            var allMetrics = await _entitiesRepository.GetAsync<MetricTick>(mt => mt.Date > request.From && mt.Date < request.To && ids.Contains(mt.MetricId), null, "Date:1", 1000, 0);

            if(allMetrics.Count() == 0)
            {
                return new QueryResult<object>()
                {
                    Result = null
                };
            }
            var startingDate = allMetrics.First().Date;

            var result = new System.Collections.Generic.Dictionary<int, Dictionary<DateTime, object>>();
            var recordCache = new Dictionary<int, List<MetricTick>>();

            foreach(var group in ids)
            {
                result.Add(group, new Dictionary<DateTime, object>());
                recordCache.Add(group, new List<MetricTick>());
            }

            var currentDate = startingDate;
            var toDate = currentDate.AddSeconds(GetSecondsAddition(request.Interval));
            foreach(var metric in allMetrics)
            {
                if(metric.Date > toDate)
                {
                    //now we calculate whats in the cache
                    foreach(var kv in recordCache)
                    {
                        if (kv.Value.Count > 0)
                        {
                            //This can be made more efficient
                            result[kv.Key].Add(currentDate, new
                            {
                                Max = kv.Value.Max(v => v.Value),
                                Min = kv.Value.Min(v => v.Value),
                                Avg = kv.Value.Average(v => v.Value)
                            });
                        }
                    }
                    //Clear the cache
                    recordCache = new Dictionary<int, List<MetricTick>>();
                }

                if(!recordCache.ContainsKey(metric.MetricId))
                {
                    recordCache.Add(metric.MetricId, new List<MetricTick>());
                }

                recordCache[metric.MetricId].Add(metric);
                currentDate = currentDate.AddSeconds(GetSecondsAddition(request.Interval));
            }

            foreach (var kv in recordCache)
            {
                if (kv.Value.Count > 0)
                {
                    //This can be made more efficient
                    result[kv.Key].Add(currentDate, new
                    {
                        Max = kv.Value.Max(v => v.Value),
                        Min = kv.Value.Min(v => v.Value),
                        Avg = kv.Value.Average(v => v.Value)
                    });
                }
            }

            Dictionary<string, Dictionary<DateTime, object>> convertedResult = new Dictionary<string, Dictionary<DateTime, object>>();

            foreach(var kv in result)
            {
                convertedResult.Add(foundMetrics.First(fm => fm.MetricId == kv.Key).MetricName, kv.Value);
            }

            return new QueryResult<object>()
            {
                Result = convertedResult
            };
        }

        public int GetSecondsAddition(char interval)
        {
            switch(interval)
            {
                case 'S':
                    return 1;
                case 'M':
                    return 1 * 60;
                case 'H':
                    return 1 * 60 * 60;
                case 'd':
                    return 1 * 60 * 60 * 24;
                case 'm':
                    return 1 * 60 * 60 *24 * 30;
                case 'Y':
                    return 1 * 60 * 60 * 24 * 365;
            }
            throw new Exception("Time interval " + interval + " is not accepted.");
        }
    }
}
