using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Workflows.Queries.GetWorkflows;
using Cindi.Domain.Entities.Workflows;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Workflows.Queries.GetWorkflows
{
    public class GetWorkflowsQueryHandler : IRequestHandler<GetWorkflowsQuery, QueryResult<List<Workflow>>>
    {
        private IWorkflowsRepository _workflowsRepository;

        public GetWorkflowsQueryHandler(IWorkflowsRepository workflowsRepository)
        {
            _workflowsRepository = workflowsRepository;
        }

        public async Task<QueryResult<List<Workflow>>> Handle(GetWorkflowsQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var foundWorkflows = await _workflowsRepository.GetWorkflowsAsync(request.Size, request.Page, request.Status);

            stopwatch.Stop();
            return new QueryResult<List<Workflow>>()
            {
                Result = foundWorkflows,
                Count = _workflowsRepository.CountWorkflows(),
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}

