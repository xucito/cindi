using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.Sequences;
using Cindi.Domain.Entities.SequencesTemplates;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.SequenceTemplates.Queries.GetSequenceTemplate
{
    public class GetSequenceTemplateQueryHandler : IRequestHandler<GetSequenceTemplateQuery, QueryResult<SequenceTemplate>>
    {
        private readonly ISequenceTemplatesRepository _sequenceTemplatesRepository;

        public GetSequenceTemplateQueryHandler(ISequenceTemplatesRepository sequenceTemplatesRepository)
        {
            _sequenceTemplatesRepository = sequenceTemplatesRepository;
        }

        public async Task<QueryResult<SequenceTemplate>> Handle(GetSequenceTemplateQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var foundSequenceTemplate = await _sequenceTemplatesRepository.GetSequenceTemplateAsync(request.Id);

            stopwatch.Stop();
            return new QueryResult<SequenceTemplate>()
            {
                Result = foundSequenceTemplate,
                Count = foundSequenceTemplate == null ? 0 : 1,
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
