using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.WorkflowTemplates.Queries.GetWorkFlowTemplate;
using Cindi.Domain.Entities.WorkflowsTemplates;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.WorkflowTemplates.Queries.GetWorkflowTemplate
{
    public class GetWorkflowTemplateQueryHandler : IRequestHandler<GetWorkflowTemplateQuery, QueryResult<WorkflowTemplate>>
    {
        private readonly IWorkflowTemplatesRepository _workflowTemplatesRepository;

        public GetWorkflowTemplateQueryHandler(IWorkflowTemplatesRepository workflowTemplatesRepository)
        {
            _workflowTemplatesRepository = workflowTemplatesRepository;
        }

        public async Task<QueryResult<WorkflowTemplate>> Handle(GetWorkflowTemplateQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var foundWorkflowTemplate = await _workflowTemplatesRepository.GetWorkflowTemplateAsync(request.Id);

            stopwatch.Stop();
            return new QueryResult<WorkflowTemplate>()
            {
                Result = foundWorkflowTemplate,
                Count = foundWorkflowTemplate == null ? 0 : 1,
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}
