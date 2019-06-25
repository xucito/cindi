using Cindi.Application.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Steps.Queries.UnencryptStepField
{
    public class UnencryptStepFieldQuery : IRequest<QueryResult<string>>
    {
        public Guid StepId { get; set; }
        public string FieldName { get; set; }
        public string UserId { get; set; }
        public string Type { get; set; }
    }
}
