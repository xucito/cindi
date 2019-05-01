using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.GlobalValues;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.GlobalValues.Queries.GetGlobalValues
{
    public class GetGlobalValuesQueryHandler : IRequestHandler<GetGlobalValuesQuery, QueryResult<IEnumerable<GlobalValue>>>
    {
        IGlobalValuesRepository _globalValuesRepository { get; set; }

        public GetGlobalValuesQueryHandler(IGlobalValuesRepository globalValuesRepository)
        {
            _globalValuesRepository = globalValuesRepository;
        }

        public async Task<QueryResult<IEnumerable<GlobalValue>>> Handle(GetGlobalValuesQuery request, CancellationToken cancellationToken)
        {
            var globalValues = await _globalValuesRepository.GetGlobalValuesAsync(request.Size, request.Page);

            return new QueryResult<IEnumerable<GlobalValue>>()
            {
                Count = globalValues.Count,
                ElapsedMs = 0,
                Result = globalValues
            };
        }
    }
}
