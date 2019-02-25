using Cindi.Application.Interfaces;
using Cindi.Application.Results;
using Cindi.Domain.Entities.JournalEntries;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Exceptions.Steps;
using Cindi.Domain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cindi.Application.Steps.Commands.AssignStep
{
    public class AssignStepCommandHandler : IRequestHandler<AssignStepCommand, CommandResult>
    {
        private readonly IStepsRepository _stepsRepository;

        public AssignStepCommandHandler(IStepsRepository stepsRepository)
        {
            _stepsRepository = stepsRepository;

        }
        public async Task<CommandResult> Handle(AssignStepCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var unassignedStep = await _stepsRepository.GetStepsAsync(StepStatuses.Unassigned, request.CompatibleStepTemplateIds);

            var step = await _stepsRepository.GetStepAsync(unassignedStep.Id);
            
            //This should not throw a error externally, the server should loop to the next one and log a error
            if (step.Status != StepStatuses.Unassigned)
            {
                throw new InvalidStepQueueException("You cannot assign step " + step.Id + " as it is not unassigned.");
            }

            await _stepsRepository.InsertJournalEntryAsync(new Domain.Entities.JournalEntries.JournalEntry()
            {
                Entity = JournalEntityTypes.Step,
                SubjectId = step.Id,
                RecordedOn = DateTime.UtcNow,
                ChainId = step.Journal.GetNextChainId(),
                Updates = new List<Domain.ValueObjects.Update>()
                {
                    new Update()
                    {
                        Type = UpdateType.Override,
                        FieldName = "status",
                        Value = StepStatuses.Assigned,
                    }

                }
            });

            stopwatch.Stop();
            return new CommandResult()
            {
                ObjectRefId = step.Id.ToString(),
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Type = CommandResultTypes.Update
            };
        }
    }
}
