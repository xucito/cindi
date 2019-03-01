using Cindi.Application.Results;
using Cindi.Domain.Entities.Sequences;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Sequences.Queries.GetSequence
{
    public class GetSequenceQuery: IRequest<QueryResult<Sequence>>
    {
        public Guid Id;
    }
}
