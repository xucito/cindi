using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Workflows.Queries.GetWorkflowSteps;
using Cindi.Domain.Entities.Steps;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Workflows.Queries.GetWorkflowSteps
{
    public class GetWorkflowStepsQueryHandler : IRequestHandler<GetWorkflowStepsQuery, QueryResult<List<Step>>>
    {
        private IWorkflowsRepository _workflowsRepository;

        public GetWorkflowStepsQueryHandler(IWorkflowsRepository workflowsRepository)
        {
            _workflowsRepository = workflowsRepository;
        }

        public async Task<QueryResult<List<Step>>> Handle(GetWorkflowStepsQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var steps = await _workflowsRepository.GetWorkflowStepsAsync(request.WorkflowId);

            stopwatch.Stop();
            return new QueryResult<List<Step>>()
            {
                Count = steps.Count,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Result = steps
            };
        }
    }
}
