using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Steps.Commands.CreateStep;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Workflows;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Global;
using Cindi.Domain.Exceptions.WorkflowTemplates;
using Cindi.Domain.Utilities;
using Cindi.Domain.ValueObjects;



using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cindi.Domain.Exceptions.Workflows;


using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Exceptions.StepTemplates;
using Microsoft.Extensions.Logging;


namespace Cindi.Application.Workflows.Commands.CreateWorkflow
{
    public class CreateWorkflowCommandHandler : IRequestHandler<CreateWorkflowCommand, CommandResult<Workflow>>
    {
        private readonly IEntitiesRepository _entitiesRepository;
        private readonly IStateMachine _stateMachine;
        private ILogger<CreateWorkflowCommandHandler> _logger;
        private IMediator _mediator;

        public CreateWorkflowCommandHandler(
            ILogger<CreateWorkflowCommandHandler> logger,
            IEntitiesRepository entitiesRepository,
            IStateMachine stateMachine,
            IMediator mediator)
        {
            _entitiesRepository = entitiesRepository;
            _stateMachine = stateMachine;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<CommandResult<Workflow>> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            WorkflowTemplate template = await _entitiesRepository.GetFirstOrDefaultAsync<WorkflowTemplate>(wft => wft.ReferenceId == request.WorkflowTemplateId);

            if (template == null)
            {
                throw new WorkflowTemplateNotFoundException("Workflow Template " + request.WorkflowTemplateId + " not found.");
            }

            if (template.InputDefinitions.Count() < request.Inputs.Count())
            {
                throw new InvalidInputsException("Invalid number of inputs, number passed was " + request.Inputs.Count() + " which is less then defined " + template.InputDefinitions.Count());
            }

            var verifiedWorkflowInputs = new Dictionary<string, object>();

            if (template.InputDefinitions != null)
            {
                foreach (var input in template.InputDefinitions)
                {
                    if (!request.Inputs.ContainsKey(input.Key))
                    {
                        throw new MissingInputException("Workflow input data is missing " + input.Key);
                    }
                }

                foreach (var input in request.Inputs)
                {
                    if (template.InputDefinitions.ContainsKey(input.Key) && template.InputDefinitions[input.Key].Type == InputDataTypes.Secret && !InputDataUtility.IsInputReference(input, out _, out _))
                    {
                        verifiedWorkflowInputs.Add(input.Key.ToLower(), SecurityUtility.SymmetricallyEncrypt((string)input.Value, _stateMachine.EncryptionKey));
                    }
                    else
                    {
                        verifiedWorkflowInputs.Add(input.Key.ToLower(), input.Value);
                    }
                }
            }

            var createdWorkflowId = Guid.NewGuid();
            var startingLogicBlock = template.LogicBlocks.Where(lb => lb.Value.Dependencies.Evaluate(new List<Step>())).ToList();

            var createdWorkflowTemplateId = await _entitiesRepository.Insert(new Domain.Entities.Workflows.Workflow()
            {
                Id = createdWorkflowId,
                WorkflowTemplateId = request.WorkflowTemplateId,
                Inputs = verifiedWorkflowInputs, //Encrypted inputs
                Name = request.Name,
                ExecutionTemplateId = request.ExecutionTemplateId,
                ExecutionScheduleId = request.ExecutionScheduleId
            });

            var workflow = await _entitiesRepository.GetFirstOrDefaultAsync<Workflow>(w => w.Id == createdWorkflowId);

            //When there are no conditions to be met

            // Needs to happen before first step is added
            DateTimeOffset WorkflowStartTime = DateTime.Now;
            foreach (var block in startingLogicBlock)
            {
                try
                {
                    foreach (var subBlock in block.Value.SubsequentSteps)
                    {
                        var newStepTemplate = await _entitiesRepository.GetFirstOrDefaultAsync<StepTemplate>(st => st.ReferenceId == subBlock.Value.StepTemplateId);

                        if (newStepTemplate == null)
                        {
                            throw new StepTemplateNotFoundException("Template " + subBlock.Value.StepTemplateId + " not found.");
                        }

                        var verifiedInputs = new Dictionary<string, object>();

                        foreach (var mapping in subBlock.Value.Mappings)
                        {
                            string mappedValue = "";
                            if ((mapping.Value.DefaultValue != null && (mapping.Value.OutputReferences == null || mapping.Value.OutputReferences.Count() == 0)) || (mapping.Value.DefaultValue != null && mapping.Value.OutputReferences.First() != null && mapping.Value.DefaultValue.Priority > mapping.Value.OutputReferences.First().Priority))
                            {
                                // Change the ID to match the output
                                verifiedInputs.Add(mapping.Key, mapping.Value.DefaultValue.Value);
                            }
                            else if (mapping.Value.OutputReferences != null)
                            {
                                verifiedInputs.Add(mapping.Key, DynamicDataUtility.GetData(request.Inputs, mapping.Value.OutputReferences.First().OutputId).Value);
                            }
                        }

                        await _mediator.Send(new CreateStepCommand()
                        {
                            StepTemplateId = subBlock.Value.StepTemplateId,
                            CreatedBy = SystemUsers.QUEUE_MANAGER,
                            Description = null,
                            Inputs = verifiedInputs,
                            WorkflowId = createdWorkflowId,
                            Name = subBlock.Key
                        });
                    }


                    workflow.CompletedLogicBlocks.Add(block.Key);
                    await _entitiesRepository.Update(workflow);
                }
                catch (Exception e)
                {
                    _logger.LogCritical("Failed to action logic block " + block.Key + " with error " + e.Message + Environment.NewLine + e.StackTrace);
                }
            }

            stopwatch.Stop();

            return new CommandResult<Workflow>()
            {
                Result = workflow,
                ObjectRefId = createdWorkflowId.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Create
            };
        }
    }
}
