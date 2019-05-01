using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.GlobalValues;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.GlobalValues.Queries.GetGlobalValue
{
    public class GetGlobalValueQueryHandler: IRequestHandler<GetGlobalValueQuery,QueryResult<GlobalValue>>
    {
        IGlobalValuesRepository _globalValuesRepository { get; set; }

        public GetGlobalValueQueryHandler(IGlobalValuesRepository globalValuesRepository)
        {
            _globalValuesRepository = globalValuesRepository;
        }

        public async Task<QueryResult<GlobalValue>> Handle(GetGlobalValueQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            var gv = await _globalValuesRepository.GetGlobalValueAsync(request.Name);

            return new QueryResult<GlobalValue>()
            {
                Count = gv != null ? 1 : 0,
                ElapsedMs = 0,
                Result = gv
            };
        }
    }
}
