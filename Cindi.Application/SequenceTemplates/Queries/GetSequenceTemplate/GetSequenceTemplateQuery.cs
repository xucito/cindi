using Cindi.Application.Results;
using Cindi.Domain.Entities.Sequences;
using Cindi.Domain.Entities.SequencesTemplates;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.SequenceTemplates.Queries.GetSequenceTemplate
{
    public class GetSequenceTemplateQuery: IRequest<QueryResult<SequenceTemplate>>
    {
        public string Id { get; set; }
    }
}
