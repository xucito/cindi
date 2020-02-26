using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.WorkflowsTemplates;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Exceptions.Global;
using Cindi.Domain.Exceptions.WorkflowTemplates;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Exceptions.StepTemplates;
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
using Microsoft.Extensions.Logging;
using Cindi.Domain.Entities.WorkflowTemplates.ValueObjects;
using Cindi.Domain.Enums;
using ConsensusCore.Node.Communication.Controllers;
using ConsensusCore.Domain.RPCs.Shard;
using Cindi.Domain.Entities.Workflows;

namespace Cindi.Application.WorkflowTemplates.Commands.CreateWorkflowTemplate
{
    public class CreateWorkflowTemplateCommandHandler : IRequestHandler<CreateWorkflowTemplateCommand, CommandResult>
    {
        private readonly IClusterRequestHandler _node;
        private ILogger<CreateWorkflowTemplateCommandHandler> Logger;
        private IEntitiesRepository _entitiesRepository;

        public CreateWorkflowTemplateCommandHandler(IEntitiesRepository entitiesRepository, IClusterRequestHandler node, ILogger<CreateWorkflowTemplateCommandHandler> logger)
        {
            _entitiesRepository = entitiesRepository;
            _node = node;
            Logger = logger;
        }

        public async Task<CommandResult> Handle(CreateWorkflowTemplateCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var existingWorkflowTemplate = await _entitiesRepository.GetFirstOrDefaultAsync<WorkflowTemplate>(wft => wft.ReferenceId == request.Name + ":" + request.Version);

            if (existingWorkflowTemplate != null)
            {
                return new CommandResult()
                {
                    ObjectRefId = existingWorkflowTemplate.ReferenceId,
                    ElapsedMs = stopwatch.ElapsedMilliseconds,
                    Type = CommandResultTypes.None
                };
            }

            //Check that all step templates exists
            foreach (var lg in request.LogicBlocks)
            {
                foreach (var ss in lg.Value.SubsequentSteps)
                {
                    var st = await _entitiesRepository.GetFirstOrDefaultAsync<StepTemplate>(stepTemplate => stepTemplate.ReferenceId == ss.Value.StepTemplateId);
                    if (st == null)
                    {
                        throw new StepTemplateNotFoundException("Template " + ss.Value.StepTemplateId + " cannot be found.");
                    }
                }
            }

            //Detect duplicates workflow step ref Id
            List<StepTemplate> allStepTemplates = new List<StepTemplate>();

            HashSet<string> stepRefs = new HashSet<string>();
            foreach (var block in request.LogicBlocks)
            {
                foreach (var subStep in block.Value.SubsequentSteps)
                {
                    if (stepRefs.Contains(subStep.Key))
                    {
                        throw new DuplicateWorkflowStepRefException("Found duplicate step refs for " + subStep.Key);
                    }
                    else
                    {
                        stepRefs.Add(subStep.Key);
                    }
                    allStepTemplates.Add(await _entitiesRepository.GetFirstOrDefaultAsync<StepTemplate>(stepTemplate => stepTemplate.ReferenceId == subStep.Value.StepTemplateId));
                }
            }

            //Dictionary<int, HashSet<int>> ValidatedSubsequentSteps = new Dictionary<int, HashSet<int>>();

            //ValidatedSubsequentSteps.Add(-1, new HashSet<int>());

            //There should be at least one pre-requisite that returns as true
            var startingLogicBlock = request.LogicBlocks.Where(lb => lb.Value.Dependencies.Evaluate(new List<Domain.Entities.Steps.Step>())).ToList();

            if (startingLogicBlock.Count() == 0)
            {
                throw new NoValidStartingLogicBlockException();
            }

            var validatedLogicBlock = true;
            List<string> validatedLogicBlockIds = new List<string>();
            validatedLogicBlockIds.AddRange(startingLogicBlock.Select(slb => slb.Key));

            Dictionary<string, ConditionGroupValidation> validations = new Dictionary<string, ConditionGroupValidation>();

            foreach (var lb in request.LogicBlocks)
            {
                validations.Add(lb.Key, null);
            }

            foreach (var startingLb in startingLogicBlock)
            {
                validations[startingLb.Key] = new ConditionGroupValidation()
                {
                    IsValid = true,
                    Reason = "Starting logic block"
                };
            }

            while (validatedLogicBlock)
            {
                //reset to true
                validatedLogicBlock = false;
                //Only evaluate unvalidatedlogicblocks
                foreach (var block in request.LogicBlocks.Where(lb => !validatedLogicBlockIds.Contains(lb.Key)))
                {
                    var validation = block.Value.Dependencies.ValidateConditionGroup(request.LogicBlocks.Where(lb => validatedLogicBlockIds.Contains(lb.Key)).Select(vlb => vlb.Value));

                    if (validations[block.Key] == null || !validations[block.Key].IsValid)
                    {
                        validations[block.Key] = validation;
                    }

                    if (validation.IsValid)
                    {
                        // Mark to re-evaluate the rest of the logicblocks
                        validatedLogicBlock = true;
                        validatedLogicBlockIds.Add(block.Key);

                        foreach (var step in block.Value.SubsequentSteps)
                        {
                            //Check whether the step template exists
                            var result = await _entitiesRepository.GetFirstOrDefaultAsync<StepTemplate>(stepTemplate => stepTemplate.ReferenceId == step.Value.StepTemplateId);

                            foreach (var mapping in step.Value.Mappings)
                            {
                                WorkflowTemplate.ValidateMapping(mapping.Value);
                            }

                            if (result == null)
                                throw new StepTemplateNotFoundException("Step Template does not exist " + step.Value.StepTemplateId);

                            var flatMappedSubsequentSteps = request.LogicBlocks.Where(lb => validatedLogicBlockIds.Contains(lb.Key)).SelectMany(lb => lb.Value.SubsequentSteps.Select(ss => ss.Key)).ToList();
                            //Add start as a valid step
                            flatMappedSubsequentSteps.Add("start");
                            foreach (var mapping in step.Value.Mappings)
                            {
                                if (mapping.Value.OutputReferences != null)
                                {
                                    foreach (var reference in mapping.Value.OutputReferences)
                                    {
                                        if (!flatMappedSubsequentSteps.Contains(reference.StepName))
                                        {
                                            throw new MissingStepException("Defined mapping for substep " + step.Key + " for mapping " + mapping.Key + " is missing  " + reference.StepName);
                                        }
                                        else
                                        {
                                            if (reference.StepName != ReservedValues.WorkflowStartStepName)
                                            {
                                                var foundBlock = request.LogicBlocks.Where(st => st.Value.SubsequentSteps.Where(ss => ss.Key == reference.StepName).Count() > 0).First();

                                                var resolvedSubstep = foundBlock.Value.SubsequentSteps.Where(ss => ss.Key == reference.StepName).First();

                                                var foundTemplates = (allStepTemplates.Where(template => template.ReferenceId == resolvedSubstep.Value.StepTemplateId));

                                                if (foundTemplates.Count() != 0)
                                                {
                                                    var foundTemplate = foundTemplates.First();
                                                    if (foundTemplate.OutputDefinitions.Where(id => id.Key.ToLower() == reference.OutputId.ToLower()).Count() == 0)
                                                    {
                                                        throw new MissingOutputException("Missing output " + reference.OutputId + " for step " + step.Key + " from step template " + foundTemplate.Id);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var foundDefinition = request.InputDefinitions.Where(id => id.Key.ToLower() == reference.OutputId.ToLower());
                                                if (foundDefinition.Count() == 0)
                                                {
                                                    throw new MissingInputException("Missing input " + reference.OutputId + " for step " + step.Key + " from workflow input.");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (mapping.Value.DefaultValue == null)
                                    {
                                        throw new MissingOutputException("Neither Value or Output References exist. If neither is required, this is a redundant output reference");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // This is done inefficiently
            /*foreach (var block in request.LogicBlocks)
            {
                foreach (var substep in block.SubsequentSteps)
                {
                    foreach (var mapping in substep.Mappings)
                    {
                        if (mapping.OutputReferences != null)
                        {
                            foreach (var reference in mapping.OutputReferences)
                            {
                                if (!ValidatedSubsequentSteps.ContainsKey(reference.WorkflowStepId))
                                {
                                    throw new MissingStepException("Defined mapping for substep " + substep.WorkflowStepId + " for mapping " + mapping.StepInputId + " is missing  " + reference.WorkflowStepId);
                                }
                                else
                                {
                                    if (reference.WorkflowStepId != -1)
                                    {
                                        var foundBlock = request.LogicBlocks.Where(st => st.SubsequentSteps.Where(ss => ss.WorkflowStepId == reference.WorkflowStepId).Count() > 0).First();

                                        var resolvedSubstep = foundBlock.SubsequentSteps.Where(ss => ss.WorkflowStepId == reference.WorkflowStepId).First();

                                        var foundTemplates = (allStepTemplates.Where(template => template.ReferenceId == resolvedSubstep.StepTemplateId));

                                        if (foundTemplates.Count() != 0)
                                        {
                                            var foundTemplate = foundTemplates.First();
                                            if (foundTemplate.InputDefinitions.Where(id => id.Key.ToLower() != reference.OutputId.ToLower()).Count() == 0)
                                            {
                                                throw new MissingInputException("Missing input " + reference.OutputId + " for step " + substep.WorkflowStepId + " from step template " + foundTemplate.Id);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var foundDefinition = request.InputDefinitions.Where(id => id.Key.ToLower() == reference.OutputId.ToLower());
                                        if (foundDefinition.Count() == 0)
                                        {
                                            throw new MissingInputException("Missing input " + reference.OutputId + " for step " + substep.WorkflowStepId + " from workflow input.");
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (mapping.DefaultValue == null)
                            {
                                throw new MissingOutputException("Neither Value or Output References exist. If neither is required, this is a redundant output reference");
                            }
                        }
                    }
                }
            }*/

            var newId = Guid.NewGuid();
            var newWorkflowTemplate = new WorkflowTemplate(newId,
                request.Name + ":" + request.Version,
                request.Description,
                request.InputDefinitions,
                request.LogicBlocks,
                request.CreatedBy,
                DateTime.UtcNow
            );

            var createdWorkflowTemplateId = await _node.Handle(new AddShardWriteOperation()
            {
                Data = newWorkflowTemplate,
                WaitForSafeWrite = true,
                Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Create
            });

            stopwatch.Stop();
            return new CommandResult()
            {
                ObjectRefId = newWorkflowTemplate.ReferenceId,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Create
            };
        }
    }
}
