﻿using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.Steps;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Steps.Queries.GetStep
{
    public class GetStepQueryHandler : IRequestHandler<GetStepQuery, QueryResult<Step>>
    {
        private readonly IStepsRepository _stepsRepository;

        public GetStepQueryHandler(IStepsRepository stepsRepository)
        {
            _stepsRepository = stepsRepository;

        }
        public async Task<QueryResult<Step>> Handle(GetStepQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var step = await _stepsRepository.GetStepAsync(request.Id);

            stopwatch.Stop();
            return new QueryResult<Step>()
            {
                Result = step,
                Count = step == null ? 0 : 1,
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}