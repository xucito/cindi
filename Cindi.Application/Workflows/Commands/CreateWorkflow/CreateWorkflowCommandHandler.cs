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
using ConsensusCore.Domain.Interfaces;
using ConsensusCore.Domain.RPCs;
using ConsensusCore.Node;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cindi.Domain.Exceptions.Workflows;

namespace Cindi.Application.Workflows.Commands.CreateWorkflow
{
    public class CreateWorkflowCommandHandler : IRequestHandler<CreateWorkflowCommand, CommandResult>
    {
        private IWorkflowsRepository _workflowsRepository;
        private IWorkflowTemplatesRepository _workflowTemplatesRepository;
        private IStepsRepository _stepsRepository;
        private IStepTemplatesRepository _stepTemplatesRepository;
        private IMediator _mediator;
        private readonly IConsensusCoreNode<CindiClusterState> _node;

        public CreateWorkflowCommandHandler(IWorkflowsRepository workflowsRepository,
            IWorkflowTemplatesRepository workflowTemplatesRepository,
            IStepsRepository stepsRepository,
            IStepTemplatesRepository stepTemplatesRepository,
            IMediator mediator,
            IConsensusCoreNode<CindiClusterState> node)
        {
            _workflowsRepository = workflowsRepository;
            _workflowTemplatesRepository = workflowTemplatesRepository;
            _stepsRepository = stepsRepository;
            _stepTemplatesRepository = stepTemplatesRepository;
            _mediator = mediator;
            _node = node;
        }

        public async Task<CommandResult> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            WorkflowTemplate template = await _workflowTemplatesRepository.GetWorkflowTemplateAsync(request.WorkflowTemplateId);

            if(template == null)
            {
                throw new WorkflowTemplateNotFoundException("Workflow Template " + request.WorkflowTemplateId + " not found.");
            }

            if(template.InputDefinitions.Count() < request.Inputs.Count())
            {
                throw new InvalidInputsException("Invalid number of inputs, number passed was " + request.Inputs.Count() + " which is less then defined " + template.InputDefinitions.Count());
            }

            if (template.InputDefinitions != null)
            {
                foreach (var inputs in template.InputDefinitions)
                {
                    if (!request.Inputs.ContainsKey(inputs.Key))
                    {
                        throw new MissingInputException("Workflow input data is missing " + inputs.Key);
                    }
                }
            }

            var createdWorkflowId = Guid.NewGuid();
            var startingLogicBlock = template.LogicBlocks.Where(lb => lb.Value.Dependencies.Evaluate(new List<Step>())).ToList();

            var createdWorkflowTemplateId = await _node.Handle(new WriteData()
            {
                Data = new Domain.Entities.Workflows.Workflow(
                createdWorkflowId,
                request.WorkflowTemplateId,
                request.Inputs,
                request.Name,
                request.CreatedBy,
                DateTime.UtcNow
            ),
                WaitForSafeWrite = true,
                Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Create
            });

            var workflow = await _workflowsRepository.GetWorkflowAsync(createdWorkflowId);

            //When there are no conditions to be met

            // Needs to happen before first step is added
            DateTimeOffset WorkflowStartTime = DateTime.Now;
            foreach (var block in startingLogicBlock)
            {
                foreach (var subBlock in block.Value.SubsequentSteps)
                {
                    var newStepTemplate = await _stepTemplatesRepository.GetStepTemplateAsync(subBlock.Value.StepTemplateId);

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

                  /*  newStep = await _stepsRepository.InsertStepAsync(newStep);

                    await _stepsRepository.InsertJournalEntryAsync(new JournalEntry()
                    {
                        SubjectId = newStep.Id,
                        ChainId = 0,
                        Entity = JournalEntityTypes.Step,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = SystemUsers.QUEUE_MANAGER,
                        Updates = new List<Update>()
                        {
                            new Update()
                            {
                                FieldName = "status",
                                Value = StepStatuses.Unassigned,
                                Type = UpdateType.Override
                            }
                        }
                    });

                    await _stepsRepository.UpsertStepMetadataAsync(newStep.Id); */
                }


                workflow.UpdateJournal(
                new JournalEntry()
                {
                    CreatedBy = request.CreatedBy,
                    CreatedOn = DateTime.UtcNow,
                    Updates = new List<Update>()
                    {
                    new Update()
                    {
                        FieldName = "completedlogicblocks",
                        Type = UpdateType.Append,
                        Value = block.Key //Add the logic block
                    }
                    }
                });

                //await _workflowsRepository.UpdateWorkflow(workflow);
                await _node.Handle(new WriteData()
                {
                    Data = workflow,
                    WaitForSafeWrite = true,
                    Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Update
                });
            }

            stopwatch.Stop();

            return new CommandResult()
            {
                ObjectRefId = createdWorkflowId.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Create
            };
        }
    }
}
