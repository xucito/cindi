using Cindi.Application.Results;
using Cindi.Domain.Entities.Sequences;
using Cindi.Domain.Entities.SequencesTemplates;
using Cindi.Persistence.SequenceTemplates;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.SequenceTemplates.Queries.GetSequenceTemplates
{
    public class GetSequenceTemplatesQueryHandler : IRequestHandler<GetSequenceTemplatesQuery, QueryResult<List<SequenceTemplate>>>
    {
        private readonly ISequenceTemplatesRepository _sequenceTemplatesRepository;

        public GetSequenceTemplatesQueryHandler(ISequenceTemplatesRepository sequenceTemplatesRepository)
        {
            _sequenceTemplatesRepository = sequenceTemplatesRepository;
        }

        public async Task<QueryResult<List<SequenceTemplate>>> Handle(GetSequenceTemplatesQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var sequenceTemplates = await _sequenceTemplatesRepository.GetSequenceTemplatesAsync(request.Page, request.Size);

            stopwatch.Stop();
            return new QueryResult<List<SequenceTemplate>>()
            {
                Result = sequenceTemplates,
                Count = _sequenceTemplatesRepository.CountSequenceTemplates(),
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
