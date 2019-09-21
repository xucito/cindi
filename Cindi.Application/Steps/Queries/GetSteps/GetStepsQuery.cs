﻿using Cindi.Application.Results;
using Cindi.Domain.Entities.Steps;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Cindi.Application.Steps.Queries.GetSteps
{
    public class GetStepsQuery: IRequest<QueryResult<List<Step>>>
    {
        public int Page { get; set; }
        public int Size { get; set; }
        public string Status { get; set; }
        [JsonIgnore]
        public List<Expression<Func<Step, object>>> Exclusions = null;
    }
}
