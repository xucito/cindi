using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services;
using Cindi.Domain.Entities.Metrics;
using Cindi.Domain.Entities.States;
using Nest;
using MediatR;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Application.Entities.Queries.GetEntity;

namespace Cindi.Application.Metrics.Queries.GetMetrics
{
    public class GetMetricsQueryHandler : IRequestHandler<GetMetricsQuery, QueryResult<object>>
    {
        ILogger<GetMetricsQueryHandler> _logger;
        ElasticClient _context;
        IMetricTicksRepository _metricTicksRepository;
        private IMediator _mediator;

        public GetMetricsQueryHandler(ILogger<GetMetricsQueryHandler> logger,
            ElasticClient context,
            IMetricTicksRepository metricTicksRepository,
            IMediator mediator)
        {
            _context = context;
            _metricTicksRepository = metricTicksRepository;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<QueryResult<object>> Handle(GetMetricsQuery request, CancellationToken cancellationToken)
        {
            var foundMetric = (await _mediator.Send(new GetEntityQuery<Metric>()
            {
                Expression = (e => e.Query(q => q.Term(f => f.MetricName, request.MetricName)))
            })).Result;

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
