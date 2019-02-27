using Cindi.Application.Results;
using Cindi.Domain.Entities.SequencesTemplates;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.SequenceTemplates.Queries.GetSequenceTemplates
{
    public class GetSequenceTemplatesQuery: IRequest<QueryResult<List<SequenceTemplate>>>
    {
        public int Page { get; set; }
        public int Size { get; set; }
    }
}
