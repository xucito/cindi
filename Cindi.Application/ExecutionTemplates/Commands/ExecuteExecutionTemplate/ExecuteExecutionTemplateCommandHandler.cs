using Cindi.Application.Exceptions;
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Steps.Commands.CreateStep;
using Cindi.Application.Workflows.Commands.CreateWorkflow;
using Cindi.Domain.Entities.ExecutionTemplates;
using Cindi.Domain.Enums;
using Cindi.Persistence.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.ExecutionTemplates.Commands.ExecuteExecutionTemplate
{
    public class ExecuteExecutionTemplateCommandHandler : IRequestHandler<ExecuteExecutionTemplateCommand, CommandResult>
    {
        public string Name { get; set; }
        public string CreatedBy { get; set; }

        private readonly IClusterStateService _clusterStateService;
        private readonly ApplicationDbContext _context;
        private IMediator _mediator;

        public ExecuteExecutionTemplateCommandHandler(
            IClusterStateService service,
            ApplicationDbContext context,
            IMediator mediator)
        {
            _clusterStateService = service;
            _context = context;
            _mediator = mediator;
        }


        public async Task<CommandResult> Handle(ExecuteExecutionTemplateCommand request, CancellationToken cancellationToken)
        {
            var executionTemplate = await _context.ExecutionTemplates.FirstOrDefaultAsync<ExecutionTemplate>(et => et.Name == request.Name);

            if (executionTemplate == null)
            {
                throw new InvalidExecutionRequestException("Execution template " + request.Name + " does not exits.");
            }

            if (executionTemplate.ExecutionTemplateType == ExecutionTemplateTypes.Step)
            {
                return await _mediator.Send(new CreateStepCommand()
                {
                    Name = executionTemplate.Name + " execution",
                    StepTemplateId = executionTemplate.ReferenceId,
                    ExecutionTemplateId = executionTemplate.Id,
                    Inputs = executionTemplate.Inputs,
                    CreatedBy = request.CreatedBy,
                    ExecutionScheduleId = request.ExecutionScheduleId
                });
            }
            else
            {
                return await _mediator.Send(new CreateWorkflowCommand()
                {
                    Name = executionTemplate.Name + " execution",
                    WorkflowTemplateId = executionTemplate.ReferenceId,
                    ExecutionTemplateId = executionTemplate.Id,
                    Inputs = executionTemplate.Inputs,
                    CreatedBy = request.CreatedBy,
                    ExecutionScheduleId = request.ExecutionScheduleId
                });
            }
        }
    }
}
