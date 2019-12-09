using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.Workflows;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Workflows.Queries.GetWorkflow
{
    public class GetWorkflowQueryHandler: IRequestHandler<GetWorkflowQuery, QueryResult<Workflow>>
    {
        private IWorkflowsRepository _workflowsRepository;

        public GetWorkflowQueryHandler(IWorkflowsRepository workflowsRepository)
        {
            _workflowsRepository = workflowsRepository;
        }

        public async Task<QueryResult<Workflow>> Handle(GetWorkflowQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var foundWorkflow = await _workflowsRepository.GetWorkflowAsync(request.Id);

            stopwatch.Stop();
            return new QueryResult<Workflow>()
            {
                Result = foundWorkflow,
                Count = foundWorkflow == null ? 0 : 1,
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
