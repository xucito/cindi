using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.ExecutionTemplates;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Global;
using Cindi.Persistence.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.ExecutionTemplates.Commands.CreateExecutionTemplate
{
    public class CreateExecutionTemplateCommandHandler : IRequestHandler<CreateExecutionTemplateCommand, CommandResult<ExecutionTemplate>>
    {
        private readonly IClusterStateService _clusterStateService;
        private readonly ApplicationDbContext _context;
        private IMediator _mediator;

        public CreateExecutionTemplateCommandHandler(
            IClusterStateService service,
            ApplicationDbContext context,
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

            ExecutionTemplate template = await _context.ExecutionTemplates.FirstOrDefaultAsync(st => st.Name == request.Name);

            if (template != null)
            {
                throw new InvalidExecutionTemplateException("Template with name " + request.Name + " is invalid.");
            }

            if (request.ExecutionTemplateType == ExecutionTemplateTypes.Step)
            {
                var stepTemplate = await _context.StepTemplates.FirstOrDefaultAsync(st => st.ReferenceId == request.ReferenceId);

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
                var workflowTemplate = await _context.WorkflowTemplates.FirstOrDefaultAsync(st => st.ReferenceId == request.ReferenceId);

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
                Name = request.Name,
                ReferenceId = request.ReferenceId,
                ExecutionTemplateType = request.ExecutionTemplateType,
                Description = request.Description,
                CreatedBy = request.CreatedBy,
                Inputs = request.Inputs
            };

            _context.Add(executionTemplate);
            await _context.SaveChangesAsync();


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
