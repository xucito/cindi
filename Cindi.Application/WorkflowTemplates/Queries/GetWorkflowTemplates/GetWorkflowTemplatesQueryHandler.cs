using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Entities.WorkflowsTemplates;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.WorkflowTemplates.Queries.GetWorkflowTemplates
{
    public class GetWorkflowTemplatesQueryHandler : IRequestHandler<GetWorkflowTemplatesQuery, QueryResult<List<WorkflowTemplate>>>
    {
        private readonly IWorkflowTemplatesRepository _workflowTemplatesRepository;

        public GetWorkflowTemplatesQueryHandler(IWorkflowTemplatesRepository workflowTemplatesRepository)
        {
            _workflowTemplatesRepository = workflowTemplatesRepository;
        }

        public async Task<QueryResult<List<WorkflowTemplate>>> Handle(GetWorkflowTemplatesQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var workflowTemplates = await _workflowTemplatesRepository.GetWorkflowTemplatesAsync(request.Page, request.Size);

            stopwatch.Stop();
            return new QueryResult<List<WorkflowTemplate>>()
            {
                Result = workflowTemplates,
                Count = _workflowTemplatesRepository.CountWorkflowTemplates(),
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
