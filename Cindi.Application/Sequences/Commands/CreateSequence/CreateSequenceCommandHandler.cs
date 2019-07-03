using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Application.Steps.Commands.CreateStep;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Sequences;
using Cindi.Domain.Entities.SequencesTemplates;
using Cindi.Domain.Entities.States;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Global;
using Cindi.Domain.Exceptions.SequenceTemplates;
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

namespace Cindi.Application.Sequences.Commands.CreateSequence
{
    public class CreateSequenceCommandHandler : IRequestHandler<CreateSequenceCommand, CommandResult>
    {
        private ISequencesRepository _sequencesRepository;
        private ISequenceTemplatesRepository _sequenceTemplatesRepository;
        private IStepsRepository _stepsRepository;
        private IStepTemplatesRepository _stepTemplatesRepository;
        private IMediator _mediator;
        private readonly IConsensusCoreNode<CindiClusterState, IBaseRepository> _node;

        public CreateSequenceCommandHandler(ISequencesRepository sequencesRepository,
            ISequenceTemplatesRepository sequenceTemplatesRepository,
            IStepsRepository stepsRepository,
            IStepTemplatesRepository stepTemplatesRepository,
            IMediator mediator,
            IConsensusCoreNode<CindiClusterState, IBaseRepository> node)
        {
            _sequencesRepository = sequencesRepository;
            _sequenceTemplatesRepository = sequenceTemplatesRepository;
            _stepsRepository = stepsRepository;
            _stepTemplatesRepository = stepTemplatesRepository;
            _mediator = mediator;
            _node = node;
        }

        public async Task<CommandResult> Handle(CreateSequenceCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            SequenceTemplate template = await _sequenceTemplatesRepository.GetSequenceTemplateAsync(request.SequenceTemplateId);

            if(template == null)
            {
                throw new SequenceTemplateNotFoundException("Sequence Template " + request.SequenceTemplateId + " not found.");
            }

            if (template.InputDefinitions != null)
            {
                foreach (var inputs in template.InputDefinitions)
                {
                    if (!request.Inputs.ContainsKey(inputs.Key))
                    {
                        throw new MissingInputException("Sequence input data is missing " + inputs.Key);
                    }
                }
            }

            var createdSequenceId = Guid.NewGuid();

            var createdSequenceTemplateId = await _node.Send(new WriteData()
            {
                Data = new Domain.Entities.Sequences.Sequence(
                createdSequenceId,
                request.SequenceTemplateId,
                request.Inputs,
                request.Name,
                request.CreatedBy,
                DateTime.UtcNow
            ),
                WaitForSafeWrite = true,
                Operation = ConsensusCore.Domain.Enums.ShardOperationOptions.Create
            });

            var startingLogicBlock = template.LogicBlocks.Where(lb => lb.PrerequisiteSteps.Count() == 0).ToList();

            // Needs to happen before first step is added
            DateTimeOffset SequenceStartTime = DateTime.Now;
            foreach (var block in startingLogicBlock)
            {
                foreach (var subBlock in block.SubsequentSteps)
                {
                    var newStepTemplate = await _stepTemplatesRepository.GetStepTemplateAsync(subBlock.StepTemplateId);

                    var verifiedInputs = new Dictionary<string, object>();


                    foreach (var mapping in subBlock.Mappings)
                    {
                        string mappedValue = "";
                        if ((mapping.DefaultValue != null && (mapping.OutputReferences == null || mapping.OutputReferences.Count() == 0)) || (mapping.DefaultValue != null && mapping.OutputReferences.First() != null && mapping.DefaultValue.Priority > mapping.OutputReferences.First().Priority))
                        {
                            // Change the ID to match the output
                            verifiedInputs.Add(mapping.StepInputId, mapping.DefaultValue.Value);
                        }
                        else if (mapping.OutputReferences != null)
                        {
                            verifiedInputs.Add(mapping.StepInputId, DynamicDataUtility.GetData(request.Inputs, mapping.OutputReferences.First().OutputId).Value);
                        }
                    }

                    await _mediator.Send(new CreateStepCommand()
                    {
                        StepTemplateId = subBlock.StepTemplateId,
                        CreatedBy = SystemUsers.QUEUE_MANAGER,
                        Description = null,
                        Inputs = verifiedInputs,
                        SequenceId = createdSequenceId,
                        StepRefId = subBlock.StepRefId,
                        Name = null
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
            }
            

            stopwatch.Stop();

            return new CommandResult()
            {
                ObjectRefId = createdSequenceId.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Create
            };
        }
    }
}
