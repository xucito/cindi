using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.SequencesTemplates;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Domain.Exceptions.Global;
using Cindi.Domain.Exceptions.SequenceTemplates;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.Exceptions.StepTemplates;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.SequenceTemplates.Commands.CreateSequenceTemplate
{
    public class CreateSequenceTemplateCommandHandler : IRequestHandler<CreateSequenceTemplateCommand, CommandResult>
    {
        private readonly ISequenceTemplatesRepository _sequenceTemplatesRepository;
        private readonly IStepTemplatesRepository _stepTemplatesRepository;

        public CreateSequenceTemplateCommandHandler(ISequenceTemplatesRepository sequenceTemplatesRepository, IStepTemplatesRepository stepTemplatesRepository)
        {
            _sequenceTemplatesRepository = sequenceTemplatesRepository;
            _stepTemplatesRepository = stepTemplatesRepository;
        }

        public async Task<CommandResult> Handle(CreateSequenceTemplateCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var existingSequenceTemplate = await _sequenceTemplatesRepository.GetSequenceTemplateAsync(request.Name + ":" + request.Version);

            if(existingSequenceTemplate != null)
            {
                return new CommandResult()
                {
                    ObjectRefId = existingSequenceTemplate.Id,
                    ElapsedMs = stopwatch.ElapsedMilliseconds,
                    Type = CommandResultTypes.None
                };
            }

            //Check that all step templates exists
            foreach (var lg in request.LogicBlocks)
            {
                foreach (var ss in lg.SubsequentSteps)
                {
                    var st = await _stepTemplatesRepository.GetStepTemplateAsync(ss.StepTemplateId);
                    if(st == null)
                    {
                        throw new StepTemplateNotFoundException("Template " + ss.StepTemplateId + " cannot be found.");
                    }
                }
            }

            //Detect duplicates sequence step ref Id
            List<StepTemplate> allStepTemplates = new List<StepTemplate>();

            HashSet<int> stepRefs = new HashSet<int>();
            foreach (var block in request.LogicBlocks)
            {
                foreach (var subStep in block.SubsequentSteps)
                {
                    if (stepRefs.Contains(subStep.StepRefId))
                    {
                        throw new DuplicateSequenceStepRefException("Found duplicate step refs for " + subStep.StepRefId);
                    }
                    else
                    {
                        stepRefs.Add(subStep.StepRefId);
                    }
                    allStepTemplates.Add(await _stepTemplatesRepository.GetStepTemplateAsync(subStep.StepTemplateId));
                }
            }

            Dictionary<int, HashSet<int>> ValidatedSubsequentSteps = new Dictionary<int, HashSet<int>>();

            ValidatedSubsequentSteps.Add(-1, new HashSet<int>());
            /*ValidatedSubsequentSteps.Add(0, new HashSet<int>());
            ValidatedSubsequentSteps.Add(1, new HashSet<int>());
            */

            //Starting Logic Block must only have have subsequent step ref with step 0
            var startingLogicBlock = request.LogicBlocks.Where(lb => lb.PrerequisiteSteps.Count() == 0);

            if (startingLogicBlock.Count() == 0)
            {
                throw new NoValidStartingLogicBlockException();
            }

            foreach (var block in request.LogicBlocks)
            {
                foreach (var step in block.SubsequentSteps)
                {
                    //Check whether the step template exists
                    var result = await _stepTemplatesRepository.GetStepTemplateAsync(step.StepTemplateId);

                    if (ValidatedSubsequentSteps.ContainsKey(step.StepRefId))
                    {
                        foreach (var prerequsiteStep in block.PrerequisiteSteps)
                        {
                            if (!ValidatedSubsequentSteps[step.StepRefId].Contains(prerequsiteStep.StepRefId))
                            {
                                ValidatedSubsequentSteps[step.StepRefId].Add(prerequsiteStep.StepRefId);
                            }
                        }
                    }
                    else
                    {
                        ValidatedSubsequentSteps.Add(step.StepRefId, new HashSet<int>());
                    }

                    foreach (var mapping in step.Mappings)
                    {
                        SequenceTemplate.ValidateMapping(mapping);
                    }

                    if (result == null)
                        throw new StepTemplateNotFoundException("Step Template does not exist " + step.StepTemplateId);
                }
            }

            foreach (var block in request.LogicBlocks)
            {
                foreach (var step in block.PrerequisiteSteps)
                {
                    //Check whether the step template exists
                    bool prerequisiteStepExists = false;

                    if (!ValidatedSubsequentSteps.ContainsKey(step.StepRefId))
                    {
                        throw new MissingStepException("Defined prerequisite step " + step.StepRefId + " missing in logic block " + block.Id);
                    }
                }

                foreach (var substep in block.SubsequentSteps)
                {
                    foreach (var mapping in substep.Mappings)
                    {
                        if (mapping.OutputReferences != null)
                        {
                            foreach (var reference in mapping.OutputReferences)
                            {
                                if (!ValidatedSubsequentSteps.ContainsKey(reference.StepRefId))
                                {
                                    throw new MissingStepException("Defined mapping for substep " + substep.StepRefId + " for mapping " + mapping.StepInputId + " is missing  " + reference.StepRefId);
                                }
                                else
                                {
                                    if (reference.StepRefId != -1)
                                    {
                                        var foundBlock = request.LogicBlocks.Where(st => st.SubsequentSteps.Where(ss => ss.StepRefId == reference.StepRefId).Count() > 0).First();

                                        var resolvedSubstep = foundBlock.SubsequentSteps.Where(ss => ss.StepRefId == reference.StepRefId).First();

                                        var foundTemplates = (allStepTemplates.Where(template => template.Id == resolvedSubstep.StepTemplateId));

                                        if (foundTemplates.Count() != 0)
                                        {
                                            var foundTemplate = foundTemplates.First();
                                            if (foundTemplate.InputDefinitions.Where(id => id.Key.ToLower() != reference.OutputId.ToLower()).Count() == 0)
                                            {
                                                throw new MissingInputException("Missing input " + reference.OutputId + " for step " + substep.StepRefId + " from step template " + foundTemplate.Id);
                                            }
                                            //NEED TO ADD TYPING CHECK FOR NEW SEQUENCE INPUT
                                            //if(foundTemplate.InputDefinitions.Where(id => id.Key != reference.OutputId).First().Value.Type != reference.)
                                        }
                                    }
                                    else
                                    {
                                        var foundDefinition = request.InputDefinitions.Where(id => id.Key.ToLower() == reference.OutputId.ToLower());
                                        if (foundDefinition.Count() == 0)
                                        {
                                            throw new MissingInputException("Missing input " + reference.OutputId + " for step " + substep.StepRefId + " from sequence input.");
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
            }


            var createdSequenceTemplateId = await _sequenceTemplatesRepository.InsertSequenceTemplateAsync(new Domain.Entities.SequencesTemplates.SequenceTemplate(
                request.Name + ":" + request.Version,
                request.Description,
                request.InputDefinitions,
                request.LogicBlocks,
                request.CreatedBy,
                DateTime.UtcNow
            ));

            stopwatch.Stop();
            return new CommandResult()
            {
                ObjectRefId = createdSequenceTemplateId.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Create
            };
        }
    }
}
