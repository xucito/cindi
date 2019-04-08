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

namespace Cindi.Application.Sequences.Queries.GetSequence
{
    public class GetSequenceQueryHandler : IRequestHandler<GetSequenceQuery, QueryResult<Sequence>>
    {
        private ISequencesRepository _sequencesRepository;

        public GetSequenceQueryHandler(ISequencesRepository sequencesRepository)
        {
            _sequencesRepository = sequencesRepository;
        }

        public async Task<QueryResult<Sequence>> Handle(GetSequenceQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var foundSequence = await _sequencesRepository.GetSequenceAsync(request.Id);

            stopwatch.Stop();
            return new QueryResult<Sequence>()
            {
                Result = foundSequence,
                Count = foundSequence == null ? 0 : 1,
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
