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
using ConsensusCore.Node.Communication.Controllers;
using ConsensusCore.Domain.RPCs.Shard;
using Cindi.Application.Services.ClusterState;
using Cindi.Domain.Entities.StepTemplates;

namespace Cindi.Application.Workflows.Commands.CreateWorkflow
{
    public class CreateWorkflowCommandHandler : IRequestHandler<CreateWorkflowCommand, CommandResult<Workflow>>
    {
        private IEntitiesRepository _entitiesRepository;
        private IMediator _mediator;
        private readonly IClusterRequestHandler _node;

        public CreateWorkflowCommandHandler(
            IEntitiesRepository entitiesRepository,
            IMediator mediator,
            IClusterRequestHandler node)
        {
            _entitiesRepository = entitiesRepository;
            _mediator = mediator;
            _node = node;
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
                        verifiedWorkflowInputs.Add(input.Key.ToLower(), SecurityUtility.SymmetricallyEncrypt((string)input.Value, ClusterStateService.GetEncryptionKey()));
                    }
                    else
                    {
                        verifiedWorkflowInputs.Add(input.Key.ToLower(), input.Value);
                    }
                }
            }

            var createdWorkflowId = Guid.NewGuid();
            var startingLogicBlock = template.LogicBlocks.Where(lb => lb.Value.Dependencies.Evaluate(new List<Step>())).ToList();

            var createdWorkflowTemplateId = await _node.Handle(new AddShardWriteOperation()
            {
                Data = new Domain.Entities.Workflows.Workflow(
                createdWorkflowId,
                request.WorkflowTemplateId,
                verifiedWorkflowInputs, //Encrypted inputs
                request.Name,
                request.CreatedBy,
                DateTime.UtcNow
            ),
                WaitForSafeWrite = true,
                Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Create
            });

            var workflow = await _entitiesRepository.GetFirstOrDefaultAsync<Workflow>(w => w.Id == createdWorkflowId);

            //When there are no conditions to be met

            // Needs to happen before first step is added
            DateTimeOffset WorkflowStartTime = DateTime.Now;
            foreach (var block in startingLogicBlock)
            {
                foreach (var subBlock in block.Value.SubsequentSteps)
                {
                    var newStepTemplate = await  _entitiesRepository.GetFirstOrDefaultAsync<StepTemplate>(st => st.ReferenceId == subBlock.Value.StepTemplateId);

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

                    /*  newStep = await _entitiesRepository.InsertStepAsync(newStep);

                      await _entitiesRepository.InsertJournalEntryAsync(new JournalEntry()
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

                      await _entitiesRepository.UpsertStepMetadataAsync(newStep.Id); */
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
                await _node.Handle(new AddShardWriteOperation()
                {
                    Data = workflow,
                    WaitForSafeWrite = true,
                    Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Update
                });
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
