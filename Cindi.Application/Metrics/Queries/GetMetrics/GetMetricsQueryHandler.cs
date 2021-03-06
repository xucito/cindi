﻿using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services;
using Cindi.Domain.Entities.Metrics;
using Cindi.Domain.Entities.States;
using ConsensusCore.Node;
using MediatR;
using Microsoft.Extensions.Logging;
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
        IMetricTicksRepository _metricTicksRepository;

        public GetMetricsQueryHandler(ILogger<GetMetricsQueryHandler> logger,
            IEntitiesRepository entitiesRepository,
            IMetricTicksRepository metricTicksRepository)
        {
            _entitiesRepository = entitiesRepository;
            _metricTicksRepository = metricTicksRepository;
            _logger = logger;
        }

        public async Task<QueryResult<object>> Handle(GetMetricsQuery request, CancellationToken cancellationToken)
        {
            var foundMetric = await _entitiesRepository.GetFirstOrDefaultAsync<Metric>(m => m.MetricName == request.MetricName);
            if (foundMetric == null)
            {
                throw new NotImplementedException("No metric " + request.MetricName);
            }

            var result = await _metricTicksRepository.GetMetricTicksAsync(request.From, request.To, foundMetric.MetricId, request.Aggs, request.Interval, null, null, request.IncludeSubcategories);

            return new QueryResult<object>()
            {
                Result = result
            };
        }
    }
}
