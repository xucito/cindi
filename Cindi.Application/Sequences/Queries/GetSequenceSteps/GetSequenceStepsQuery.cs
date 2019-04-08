using Cindi.Application.Results;
using Cindi.Domain.Entities.Steps;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Sequences.Queries.GetSequenceSteps
{
    public class GetSequenceStepsQuery: IRequest<QueryResult<List<Step>>>
    {
        public Guid SequenceId { get; set; }
    }
}
