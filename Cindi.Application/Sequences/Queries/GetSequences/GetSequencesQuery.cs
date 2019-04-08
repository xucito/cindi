using Cindi.Application.Results;
using Cindi.Domain.Entities.Sequences;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Sequences.Queries.GetSequences
{
    public class GetSequencesQuery: IRequest<QueryResult<List<Sequence>>>
    {
        public int Page { get; set; }
        public int Size { get; set; }
        public string Status { get; set; }
    }
}
