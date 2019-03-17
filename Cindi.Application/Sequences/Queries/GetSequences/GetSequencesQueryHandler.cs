using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.Sequences;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Sequences.Queries.GetSequences
{
    public class GetSequencesQueryHandler : IRequestHandler<GetSequencesQuery, QueryResult<List<Sequence>>>
    {
        private ISequencesRepository _sequencesRepository;

        public GetSequencesQueryHandler(ISequencesRepository sequencesRepository)
        {
            _sequencesRepository = sequencesRepository;
        }

        public async Task<QueryResult<List<Sequence>>> Handle(GetSequencesQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var foundSequences = await _sequencesRepository.GetSequencesAsync(request.Size, request.Page, request.Status);

            stopwatch.Stop();
            return new QueryResult<List<Sequence>>()
            {
                Result = foundSequences,
                Count = _sequencesRepository.CountSequences(),
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}

