using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Sequences;
using Cindi.Domain.Entities.SequencesTemplates;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Enums;
using Cindi.Domain.Exceptions.Global;
using Cindi.Domain.Exceptions.SequenceTemplates;
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

namespace Cindi.Application.Sequences.Commands.CreateSequence
{
    public class CreateSequenceCommandHandler : IRequestHandler<CreateSequenceCommand, CommandResult>
    {
        private ISequencesRepository _sequencesRepository;
        private ISequenceTemplatesRepository _sequenceTemplatesRepository;
        private IStepsRepository _stepsRepository;

        public CreateSequenceCommandHandler(ISequencesRepository sequencesRepository,
            ISequenceTemplatesRepository sequenceTemplatesRepository,
            IStepsRepository stepsRepository)
        {
            _sequencesRepository = sequencesRepository;
            _sequenceTemplatesRepository = sequenceTemplatesRepository;
            _stepsRepository = stepsRepository;
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

            var createdSequence = await _sequencesRepository.InsertSequenceAsync(new Domain.Entities.Sequences.Sequence()
            {
                Id = Guid.NewGuid(),
                SequenceTemplateId = request.SequenceTemplateId,
                Inputs = request.Inputs,
                CreatedOn = DateTime.UtcNow,
                Name = request.Name
            });

            await _sequencesRepository.InsertJournalEntryAsync(new JournalEntry()
            {
                SubjectId = createdSequence.Id,
                ChainId = 0,
                Entity = JournalEntityTypes.Sequence,
                CreatedBy = SystemUsers.QUEUE_MANAGER,
                CreatedOn = DateTime.UtcNow,
                Updates = new List<Update>()
                {
                    new Update()
                    {
                        FieldName = "status",
                        Value = SequenceStatuses.Started,
                        Type = UpdateType.Override
                    }
                }
            });

            var startingLogicBlock = template.LogicBlocks.Where(lb => lb.PrerequisiteSteps.Count() == 0).ToList();

            // Needs to happen before first step is added
            DateTimeOffset SequenceStartTime = DateTime.Now;
            foreach (var block in startingLogicBlock)
            {
                foreach (var subBlock in block.SubsequentSteps)
                {
                    var newStep = new Step()
                    {
                        SequenceId = createdSequence.Id,
                        StepRefId = subBlock.StepRefId,
                        Inputs = new Dictionary<string, object>(),
                        StepTemplateId = subBlock.StepTemplateId,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = SystemUsers.QUEUE_MANAGER,
                    };

                    foreach (var mapping in subBlock.Mappings)
                    {
                        string mappedValue = "";
                        if ((mapping.DefaultValue != null && (mapping.OutputReferences == null || mapping.OutputReferences.Count() == 0)) || (mapping.DefaultValue != null && mapping.OutputReferences.First() != null && mapping.DefaultValue.Priority > mapping.OutputReferences.First().Priority))
                        {
                            // Change the ID to match the output
                            newStep.Inputs.Add(mapping.StepInputId, mapping.DefaultValue.Value);
                        }
                        else if (mapping.OutputReferences != null)
                        {
                            newStep.Inputs.Add(mapping.StepInputId, DynamicDataUtility.GetData(request.Inputs, mapping.OutputReferences.First().OutputId).Value);
                        }
                    }



                    newStep = await _stepsRepository.InsertStepAsync(newStep);

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

                    await _stepsRepository.UpsertStepMetadataAsync(newStep.Id);
                }
            }


            await _sequencesRepository.UpsertSequenceMetadataAsync(createdSequence.Id);

            stopwatch.Stop();

            return new CommandResult()
            {
                ObjectRefId = createdSequence.Id.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Create
            };
        }
    }
}
