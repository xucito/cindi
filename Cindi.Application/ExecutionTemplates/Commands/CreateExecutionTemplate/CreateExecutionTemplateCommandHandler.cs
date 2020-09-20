
using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Services.ClusterOperation;
using Cindi.Domain.Entities.ExecutionTemplates;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Global;
using ConsensusCore.Domain.RPCs.Shard;
using ConsensusCore.Node.Communication.Controllers;
using MediatR;
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
        private readonly ClusterService _clusterService;

        public CreateExecutionTemplateCommandHandler(
            IClusterStateService service,
            ClusterService clusterService)
        {
            _clusterService = clusterService;
            _clusterStateService = service;
        }

        public async Task<CommandResult<ExecutionTemplate>> Handle(CreateExecutionTemplateCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            ExecutionTemplate template = await _clusterService.GetFirstOrDefaultAsync<ExecutionTemplate>(st => st.Name == request.Name);

            if (template != null)
            {
                throw new InvalidExecutionTemplateException("Template with name " + request.Name + " is invalid.");
            }

            if (request.ExecutionTemplateType == ExecutionTemplateTypes.Step)
            {
                var stepTemplate = await _clusterService.GetFirstOrDefaultAsync<StepTemplate>(st => st.ReferenceId == request.ReferenceId);

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
                var workflowTemplate = await _clusterService.GetFirstOrDefaultAsync<WorkflowTemplate>(st => st.ReferenceId == request.ReferenceId);

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
                Inputs = request.Inputs,
                Version = 0
            };

            var result = await _clusterService.AddWriteOperation(new EntityWriteOperation<ExecutionTemplate>()
            {
                Data = executionTemplate,
                Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Create,
                User = request.CreatedBy
            });


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
