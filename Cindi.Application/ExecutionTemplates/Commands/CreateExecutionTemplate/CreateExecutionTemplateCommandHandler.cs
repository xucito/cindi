using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.ExecutionTemplates;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Global;
using Nest;
using MediatR;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cindi.Application.Entities.Queries.GetEntity;
using Cindi.Application.Utilities;

namespace Cindi.Application.ExecutionTemplates.Commands.CreateExecutionTemplate
{
    public class CreateExecutionTemplateCommandHandler : IRequestHandler<CreateExecutionTemplateCommand, CommandResult<ExecutionTemplate>>
    {
        private readonly IClusterStateService _clusterStateService;
        private readonly ElasticClient _context;
        private IMediator _mediator;

        public CreateExecutionTemplateCommandHandler(
            IClusterStateService service,
            ElasticClient context,
            IMediator mediator)
        {
            _clusterStateService = service;
            _context = context;
            _mediator = mediator;
        }

        public async Task<CommandResult<ExecutionTemplate>> Handle(CreateExecutionTemplateCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            ExecutionTemplate template = (await _mediator.Send(new GetEntityQuery<ExecutionTemplate>()
            {
                Expression = (e => e.Query(q => q.Term(f => f.Field( et => et.Name.Suffix("keyword")).Value(request.Name))))
            })).Result;

            if (template != null)
            {
                throw new InvalidExecutionTemplateException("Template with name " + request.Name + " is invalid.");
            }

            if (request.ExecutionTemplateType == ExecutionTemplateTypes.Step)
            {
                var stepTemplate = (await _mediator.Send(new GetEntityQuery<StepTemplate>()
                {
                    Expression = (e => e.Query(q => q.Term(f => f.Field(a => a.ReferenceId.Suffix("keyword")).Value(request.ReferenceId))))
                })).Result;

                if (stepTemplate == null)
                {
                    throw new InvalidExecutionTemplateException("Failed to create execution template as " + request.ReferenceId + " does not exist.");
                }
                else
                {
                    foreach (var input in request.Inputs)
                    {
                        if (!stepTemplate.IsStepInputValid(input))
                        {
                            throw new InvalidExecutionTemplateException("Input " + input.Key + " is invalid.");
                        }
                    }
                }
            }
            else if (request.ExecutionTemplateType == ExecutionTemplateTypes.Workflow)
            {
                var workflowTemplate = (await _mediator.Send(new GetEntityQuery<WorkflowTemplate>()
                {
                    Expression = (e => e.Query(q => q.Term(f => f.Field(a => a.ReferenceId.Suffix("keyword")).Value(request.ReferenceId))))
                })).Result;

                if (workflowTemplate == null)
                {
                    throw new InvalidExecutionTemplateException("Failed to create execution template as " + request.ReferenceId + " does not exist.");
                }
                else
                {
                    foreach (var input in request.Inputs)
                    {
                        if (!workflowTemplate.IsWorkflowInputValid(input))
                        {
                            throw new InvalidExecutionTemplateException("Input " + input.Key + " is invalid.");
                        }
                    }
                }
            }
            else
            {
                throw new InvalidExecutionTemplateException("Execution Template Type is Invalid.");
            }



            var executionTemplate = new ExecutionTemplate()
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                ReferenceId = request.ReferenceId,
                ExecutionTemplateType = request.ExecutionTemplateType,
                Description = request.Description,
                CreatedBy = request.CreatedBy,
                Inputs = request.Inputs
            };

            ;

            if ((await _context.IndexDocumentAsync(executionTemplate)).IsValid)
            {
                await _context.MakeSureIsQueryable<ExecutionTemplate>(executionTemplate.Id);
            }

            stopwatch.Stop();
            return new CommandResult<ExecutionTemplate>()
            {
                ObjectRefId = executionTemplate.Id.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Create,
                Result = executionTemplate
            };
        }
    }
}
