using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.Steps;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Sequences.Queries.GetSequenceSteps
{
    public class GetSequenceStepsQueryHandler : IRequestHandler<GetSequenceStepsQuery, QueryResult<List<Step>>>
    {
        private ISequencesRepository _sequencesRepository;

        public GetSequenceStepsQueryHandler(ISequencesRepository sequencesRepository)
        {
            _sequencesRepository = sequencesRepository;
        }

        public async Task<QueryResult<List<Step>>> Handle(GetSequenceStepsQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var steps = await _sequencesRepository.GetSequenceStepsAsync(request.SequenceId);

            stopwatch.Stop();
            return new QueryResult<List<Step>>()
            {
                Count = steps.Count,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Result = steps
            };
        }
    }
}
